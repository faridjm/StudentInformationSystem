using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SIS.BLL.Services.DTOs.Teacher;
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
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly IEmailService _emailService;

        public TeacherService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
            IActivityLogger activityLogger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _emailService = emailService;
        }

        public async Task<IEnumerable<TeacherListDto>> GetAllTeachersAsync()
        {
            var teachers = await _unitOfWork.Teachers.GetAllWithUsersAsync();
            return teachers.Select(t => new TeacherListDto
            {
                Id = t.Id, FullName = t.AppUser?.FullName ?? "",
                FinCode = t.AppUser?.FinCode, Email = t.AppUser?.Email ?? "",
                EmployeeNumber = t.EmployeeNumber, Title = t.Title,
                IsActive = t.AppUser?.IsActive ?? false,
                GroupCount = t.Groups?.Count ?? 0
            });
        }

        public async Task<TeacherDetailDto?> GetTeacherByIdAsync(int id)
        {
            var t = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(id);
            return t == null ? null : MapToDetail(t);
        }

        public async Task<TeacherDetailDto?> GetTeacherByUserIdAsync(string userId)
        {
            var t = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(userId);
            return t == null ? null : MapToDetail(t);
        }

        public async Task CreateTeacherAsync(CreateTeacherDto dto)
        {
            if (!string.IsNullOrEmpty(dto.FinCode))
                if (await _userManager.Users.AnyAsync(u => u.FinCode == dto.FinCode))
                    throw new ConflictException("Bu FIN kod artıq istifadədədir");

            var user = new AppUser
            {
                FullName = dto.FullName.ToTitleCase(), FinCode = dto.FinCode,
                BirthDate = dto.BirthDate,
                Email = dto.Email, UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber, IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, RoleConstants.Teacher);

            var count = await _unitOfWork.Teachers.CountAsync(t => !t.IsDeleted);
            var teacher = new Teacher
            {
                AppUserId = user.Id,
                EmployeeNumber = EmployeeNumberHelper.Generate(DateTime.Now.Year, count + 1),
                Title = null, HireDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Teachers.AddAsync(teacher);
            await _unitOfWork.SaveAsync();

            user.UserName = teacher.EmployeeNumber;
            user.NormalizedUserName = teacher.EmployeeNumber.ToUpper();
            await _userManager.UpdateAsync(user);

            _ = _emailService.SendWelcomeAsync(dto.Email, dto.FullName, teacher.EmployeeNumber, dto.Password, "Teacher");
            await _activityLogger.LogAsync("Create", "Teacher", teacher.Id.ToString(), null, new { dto.FullName, dto.Email });
        }

        public async Task UpdateTeacherAsync(UpdateTeacherDto dto)
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(dto.Id)
                ?? throw new NotFoundException("Müəllim tapılmadı");

            teacher.AppUser.FullName = dto.FullName.ToTitleCase();
            teacher.AppUser.FinCode = dto.FinCode;
            teacher.AppUser.PhoneNumber = dto.PhoneNumber;
            teacher.AppUser.BirthDate = dto.BirthDate;
            await _userManager.UpdateAsync(teacher.AppUser);
            teacher.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Teachers.Update(teacher);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteTeacherAsync(int id)
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(id)
                ?? throw new NotFoundException("Müəllim tapılmadı");

            teacher.AppUser.Email = $"deleted_{id}_{teacher.AppUser.Email}";
            teacher.AppUser.UserName = teacher.AppUser.Email;
            teacher.AppUser.IsActive = false;
            await _userManager.UpdateAsync(teacher.AppUser);

            _unitOfWork.Teachers.SoftDelete(teacher);
            await _unitOfWork.SaveAsync();
        }

        private static TeacherDetailDto MapToDetail(Teacher t) => new()
        {
            Id = t.Id, AppUserId = t.AppUserId,
            FullName = t.AppUser?.FullName ?? "", FinCode = t.AppUser?.FinCode,
            Email = t.AppUser?.Email ?? "", PhoneNumber = t.AppUser?.PhoneNumber,
            EmployeeNumber = t.EmployeeNumber, Title = t.Title,
            BirthDate = t.AppUser?.BirthDate, IsActive = t.AppUser?.IsActive ?? false,
            CreatedAt = t.CreatedAt
        };
    }
}
