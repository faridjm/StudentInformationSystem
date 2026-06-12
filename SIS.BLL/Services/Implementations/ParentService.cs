using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIS.BLL.Services.DTOs.Parent;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// Valideyn hesablarının yaradılması/bağlanması (admin) və çıxış yoxlaması.
    /// Mövcud student/teacher/identity logic-ə toxunmur — yalnız Parent rolu + ParentStudent əlaqəsi.
    /// </summary>
    public class ParentService : IParentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAuditLogService _audit;

        public ParentService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IAuditLogService audit)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<ParentCreatedResultDto> CreateParentForStudentAsync(int studentId, CreateParentForStudentDto dto)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId)
                ?? throw new NotFoundException("Tələbə tapılmadı");

            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new BadRequestException("Valideynin ad-soyadı tələb olunur");

            var digits = ExtractDigits(student.StudentNumber);

            // Email: verilibsə validate, yoxdursa auto-generate
            string email;
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                email = dto.Email.Trim().ToLowerInvariant();
                if (await _userManager.Users.AnyAsync(u => u.Email == email))
                    throw new ConflictException("Bu email ilə istifadəçi artıq mövcuddur. Onu \"Mövcud valideyni bağla\" ilə link edin.");
            }
            else
            {
                email = await GenerateUniqueParentEmailAsync(digits, dto.RelationshipType);
            }

            // Parol: verilibsə istifadə et, yoxdursa auto (Parent009!)
            var password = string.IsNullOrWhiteSpace(dto.Password)
                ? $"Parent{digits}!"
                : dto.Password;

            var user = new AppUser
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                UserName = email,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            try
            {
                await _userManager.AddToRoleAsync(user, RoleConstants.Parent);

                await _unitOfWork.ParentStudents.AddAsync(new ParentStudent
                {
                    ParentUserId = user.Id,
                    StudentId = studentId,
                    RelationshipType = dto.RelationshipType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user); // rollback
                throw;
            }

            await _audit.LogAsync("CreateParentAccount", "ParentStudent", user.Id,
                $"Admin created parent account {email} for student {student.StudentNumber}.");
            await _audit.LogAsync("LinkParentToStudent", "ParentStudent", user.Id,
                $"Parent {email} linked to student {student.StudentNumber}.");

            return new ParentCreatedResultDto
            {
                ParentUserId = user.Id,
                Email = email,
                TemporaryPassword = password,
                StudentName = student.AppUser?.FullName ?? ""
            };
        }

        public async Task LinkExistingParentAsync(int studentId, LinkExistingParentDto dto)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId)
                ?? throw new NotFoundException("Tələbə tapılmadı");

            AppUser? parent = null;
            if (!string.IsNullOrWhiteSpace(dto.ParentUserId))
                parent = await _userManager.FindByIdAsync(dto.ParentUserId);
            else if (!string.IsNullOrWhiteSpace(dto.Email))
                parent = await _userManager.FindByEmailAsync(dto.Email.Trim().ToLowerInvariant());

            if (parent == null)
                throw new NotFoundException("Bu email/ID ilə istifadəçi tapılmadı");

            // Dublikat əlaqə yoxlaması
            var existing = await _unitOfWork.ParentStudents.GetRelationAsync(parent.Id, studentId);
            if (existing != null)
                throw new ConflictException("Bu valideyn artıq həmin tələbəyə bağlıdır");

            // İstifadəçinin Parent rolu yoxdursa əlavə et (mövcud rollarına toxunmadan)
            if (!await _userManager.IsInRoleAsync(parent, RoleConstants.Parent))
                await _userManager.AddToRoleAsync(parent, RoleConstants.Parent);

            await _unitOfWork.ParentStudents.AddAsync(new ParentStudent
            {
                ParentUserId = parent.Id,
                StudentId = studentId,
                RelationshipType = dto.RelationshipType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("LinkParentToStudent", "ParentStudent", parent.Id,
                $"Existing parent {parent.Email} linked to student {student.StudentNumber}.");
        }

        public async Task SetRelationActiveAsync(int studentId, string parentUserId, bool isActive)
        {
            var relation = await _unitOfWork.ParentStudents.GetRelationAsync(parentUserId, studentId)
                ?? throw new NotFoundException("Valideyn əlaqəsi tapılmadı");

            relation.IsActive = isActive;
            relation.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.ParentStudents.Update(relation);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync(isActive ? "EnableParentStudentRelation" : "DisableParentStudentRelation",
                "ParentStudent", parentUserId,
                $"Parent relation for student {studentId} {(isActive ? "enabled" : "disabled")}.");
        }

        public async Task<List<ParentInfoDto>> GetParentsForStudentAsync(int studentId)
        {
            var relations = await _unitOfWork.ParentStudents.GetByStudentAsync(studentId);
            return relations.Select(ps => new ParentInfoDto
            {
                ParentUserId = ps.ParentUserId,
                FullName = ps.ParentUser?.FullName ?? "",
                Email = ps.ParentUser?.Email,
                Phone = ps.ParentUser?.PhoneNumber,
                RelationshipType = ps.RelationshipType,
                IsActive = ps.IsActive,
                CreatedAt = ps.CreatedAt
            }).ToList();
        }

        public async Task<List<ParentChildDto>> GetChildrenAsync(string parentUserId)
        {
            if (string.IsNullOrEmpty(parentUserId)) return new();
            var relations = await _unitOfWork.ParentStudents.GetByParentAsync(parentUserId, onlyActive: true);
            return relations.Select(ps => new ParentChildDto
            {
                StudentId = ps.StudentId,
                FullName = ps.Student?.AppUser?.FullName ?? "",
                StudentNumber = ps.Student?.StudentNumber ?? "",
                RelationshipType = ps.RelationshipType
            }).ToList();
        }

        public async Task EnsureParentCanAccessAsync(string parentUserId, int studentId)
        {
            if (string.IsNullOrEmpty(parentUserId)
                || !await _unitOfWork.ParentStudents.HasActiveAccessAsync(parentUserId, studentId))
                throw new UnauthorizedAccessException("Bu tələbəyə çıxış icazəniz yoxdur");
        }

        // ── köməkçilər ──

        private static string ExtractDigits(string studentNumber)
        {
            var digits = new string((studentNumber ?? "").Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? "000" : digits;
        }

        private async Task<string> GenerateUniqueParentEmailAsync(string digits, Entities.Enums.RelationshipType rel)
        {
            string baseEmail = $"parent{digits}@sis.az";
            if (!await _userManager.Users.AnyAsync(u => u.Email == baseEmail))
                return baseEmail;

            // Relationship əsaslı variant
            var relPrefix = rel.ToString().ToLowerInvariant(); // mother/father/guardian/other
            string relEmail = $"{relPrefix}{digits}@sis.az";
            if (!await _userManager.Users.AnyAsync(u => u.Email == relEmail))
                return relEmail;

            // Nömrələnmiş variant
            for (int i = 1; i <= 50; i++)
            {
                var candidate = $"parent{digits}-{i}@sis.az";
                if (!await _userManager.Users.AnyAsync(u => u.Email == candidate))
                    return candidate;
            }
            return $"parent{digits}-{Guid.NewGuid():N}@sis.az";
        }
    }
}
