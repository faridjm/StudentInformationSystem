using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SIS.BLL.Services.DTOs.Student;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.Common.Extensions;
using SIS.Common.Helpers;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _audit;
        private readonly IInstitutionalEmailService _institutionalEmail;
        private readonly IAcademicPerformanceService _performance;
        private readonly IParentService _parentService;

        public StudentService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
            IActivityLogger activityLogger, IEmailService emailService, IAuditLogService audit,
            IInstitutionalEmailService institutionalEmail, IAcademicPerformanceService performance,
            IParentService parentService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _audit = audit;
            _institutionalEmail = institutionalEmail;
            _performance = performance;
            _parentService = parentService;
        }

        public async Task<IEnumerable<StudentListDto>> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.Students.GetAllWithUsersAsync();
            return students.Select(s => new StudentListDto
            {
                Id = s.Id, FullName = s.AppUser?.FullName ?? "",
                FinCode = s.AppUser?.FinCode, Email = s.AppUser?.Email ?? "",
                StudentNumber = s.StudentNumber, EnrollmentYear = s.EnrollmentYear,
                CurrentGPA = s.CurrentGPA, IsActive = s.AppUser?.IsActive ?? false
            });
        }

        public async Task<StudentDetailDto?> GetStudentByIdAsync(int id)
        {
            var s = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            if (s == null) return null;
            return MapToDetail(s);
        }

        public async Task<StudentDetailDto?> GetStudentByUserIdAsync(string userId)
        {
            var s = await _unitOfWork.Students.GetStudentByUserIdAsync(userId);
            if (s == null) return null;
            return MapToDetail(s);
        }

        public async Task CreateStudentAsync(CreateStudentDto dto)
        {
            // ── Validation ──
            if (!string.IsNullOrEmpty(dto.FinCode))
                if (await _userManager.Users.AnyAsync(u => u.FinCode == dto.FinCode && u.IsActive))
                    throw new ConflictException("Bu FIN kod artıq istifadədədir");

            // Email
            string resolvedEmail;
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                var parts = dto.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var firstName = parts[0];
                var lastName  = parts.Length > 1 ? parts[1] : parts[0];
                resolvedEmail = await _institutionalEmail.GenerateEmailAsync(firstName, lastName);
            }
            else
            {
                resolvedEmail = dto.Email.Trim().ToLowerInvariant();
                if (await _userManager.Users.AnyAsync(u => u.Email == resolvedEmail))
                    throw new ConflictException("Bu email artıq istifadədədir");
            }

            // Phone
            var normalizedPhone = await Services.Helpers.PhoneUniqueValidator.ValidateAndNormalizeAsync(_userManager, dto.PhoneNumber);

            // ── Unique StudentNumber generation (silinmişlər daxil) ──
            var existingNumbers = await _unitOfWork.Students.GetAllStudentNumbersAsync();
            var studentNumber = StudentNumberHelper.GenerateNext(existingNumbers);

            // ── Transaction: AppUser + Role + Student ──
            // UserManager öz transaction-unu idarə edir, amma əgər Student save uğursuz olarsa
            // yaradılmış user-i silməliyik (rollback).
            var user = new AppUser
            {
                FullName = dto.FullName.ToTitleCase(), FinCode = dto.FinCode,
                BirthDate = dto.BirthDate,
                Email = resolvedEmail, UserName = resolvedEmail,
                PhoneNumber = normalizedPhone,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            Student student;
            try
            {
                await _userManager.AddToRoleAsync(user, RoleConstants.Student);

                student = new Student
                {
                    AppUserId = user.Id,
                    StudentNumber = studentNumber,
                    EnrollmentYear = dto.EnrollmentYear,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.SaveAsync();

                // UserName-i StudentNumber ilə yenilə
                user.UserName = student.StudentNumber;
                user.NormalizedUserName = student.StudentNumber.ToUpper();
                await _userManager.UpdateAsync(user);

                _ = _emailService.SendWelcomeAsync(resolvedEmail, dto.FullName, student.StudentNumber, dto.Password, "Student");
                await _activityLogger.LogAsync("Create", "Student", student.Id.ToString(), null, new { dto.FullName, Email = resolvedEmail });
                await _audit.LogAsync("Create", "Student", student.Id.ToString(),
                    $"Tələbə yaradıldı: {student.AppUser?.FullName ?? dto.FullName} (№ {student.StudentNumber})");
            }
            catch
            {
                // Student save və ya role assignment uğursuz → yaradılmış AppUser-i sil ki, partial data qalmasın
                await _userManager.DeleteAsync(user);
                throw;
            }

            // ── Optional valideyn hesabı (tələbə artıq yaradılıb; uğursuzluq tələbəni rollback etmir) ──
            if (dto.CreateParentAccount)
            {
                try
                {
                    await _parentService.CreateParentForStudentAsync(student.Id, new DTOs.Parent.CreateParentForStudentDto
                    {
                        FullName = string.IsNullOrWhiteSpace(dto.ParentFullName)
                            ? $"{dto.FullName} (valideyn)" : dto.ParentFullName!,
                        Email = dto.ParentEmail,
                        Phone = dto.ParentPhone,
                        RelationshipType = dto.ParentRelationshipType,
                        Password = dto.ParentPassword
                    });
                }
                catch (Exception ex)
                {
                    // Tələbə qalır; admin sonradan Parents tab-dan əlavə edə bilər
                    throw new BadRequestException(
                        $"Tələbə yaradıldı, lakin valideyn hesabı yaradıla bilmədi: {ex.Message}. " +
                        "Valideyni sonradan tələbə detalından əlavə edə bilərsiniz.");
                }
            }
        }

        public async Task UpdateStudentAsync(UpdateStudentDto dto)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(dto.Id)
                ?? throw new NotFoundException("Tələbə tapılmadı");

            student.AppUser.FullName = dto.FullName.ToTitleCase();
            student.AppUser.FinCode = dto.FinCode;
            student.AppUser.PhoneNumber = await Services.Helpers.PhoneUniqueValidator.ValidateAndNormalizeAsync(
                _userManager, dto.PhoneNumber, student.AppUser.Id);
            await _userManager.UpdateAsync(student.AppUser);

            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Update", "Student", student.Id.ToString(),
                $"Tələbə məlumatı yeniləndi: {student.AppUser.FullName} (№ {student.StudentNumber})");
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(id)
                ?? throw new NotFoundException("Tələbə tapılmadı");

            student.AppUser.Email = $"deleted_{id}_{student.AppUser.Email}";
            student.AppUser.UserName = student.AppUser.Email;
            student.AppUser.IsActive = false;
            student.AppUser.FinCode = null; // FIN kodu azad et ki, eyni şəxs yenidən qeydiyyatdan keçə bilsin
            await _userManager.UpdateAsync(student.AppUser);

            // Tələbənin qeydiyyatlarını da soft-delete et (qrup tələbə sayı düzgün olsun)
            var enrollments = await _unitOfWork.Enrollments.GetByStudentAsync(id);
            foreach (var enrollment in enrollments.Where(e => !e.IsDeleted))
                _unitOfWork.Enrollments.SoftDelete(enrollment);

            _unitOfWork.Students.SoftDelete(student);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateGPAAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
            if (student == null) return;

            // Hər aktiv enrollment-in performansını mərkəzi servicedən al,
            // sonra GPA-nı bu enrollment-lərin orta official grade-ı üzrə hesabla.
            var enrollments = await _unitOfWork.Enrollments.GetByStudentAsync(studentId);
            decimal sumGrade = 0; int count = 0;
            foreach (var enrollment in enrollments.Where(e => e.Status == SIS.Entities.Enums.EnrollmentStatus.Active))
            {
                var perf = await _performance.GetEnrollmentPerformanceAsync(enrollment.Id);
                sumGrade += perf.OfficialGrade;
                count++;
            }

            var avgPct = count > 0 ? sumGrade / count : 0m;
            student.CurrentGPA = _performance.CalculateGpa(avgPct);
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveAsync();
        }

        private static StudentDetailDto MapToDetail(Student s) => new()
        {
            Id = s.Id, AppUserId = s.AppUserId,
            FullName = s.AppUser?.FullName ?? "", FinCode = s.AppUser?.FinCode,
            Email = s.AppUser?.Email ?? "", PhoneNumber = s.AppUser?.PhoneNumber,
            StudentNumber = s.StudentNumber, EnrollmentYear = s.EnrollmentYear,
            CurrentGPA = s.CurrentGPA, IsActive = s.AppUser?.IsActive ?? false,
            CreatedAt = s.CreatedAt
        };
    }
}
