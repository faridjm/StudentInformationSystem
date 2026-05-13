using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SIS.BLL.Services.DTOs.Teacher;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.Common.Extensions;
using SIS.Common.Helpers;
using SIS.Common.Pagination;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IActivityLogger _activityLogger;

        public TeacherService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager,
            IActivityLogger activityLogger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _activityLogger = activityLogger;
        }

        public async Task<PagedResult<TeacherListDto>> GetAllTeachersAsync(PagedRequest request)
        {
            var pagedTeachers = await _unitOfWork.Teachers.GetPagedAsync(request);

            return new PagedResult<TeacherListDto>
            {
                Items = _mapper.Map<IEnumerable<TeacherListDto>>(pagedTeachers.Items),
                TotalCount = pagedTeachers.TotalCount,
                Page = pagedTeachers.Page,
                PageSize = pagedTeachers.PageSize
            };
        }

        public async Task<TeacherDetailDto?> GetTeacherByIdAsync(int id)
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(id);
            return _mapper.Map<TeacherDetailDto>(teacher);
        }

        public async Task<TeacherDetailDto?> GetTeacherByUserIdAsync(string userId)
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(userId);
            return _mapper.Map<TeacherDetailDto>(teacher);
        }

        public async Task<IEnumerable<TeacherListDto>> GetTeachersByDepartmentAsync(int departmentId)
        {
            var teachers = await _unitOfWork.Teachers.GetTeachersByDepartmentAsync(departmentId);
            return _mapper.Map<IEnumerable<TeacherListDto>>(teachers);
        }

        public async Task CreateTeacherAsync(CreateTeacherDto dto)
        {
            var user = new AppUser
            {
                FullName = dto.FullName.ToTitleCase(),
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, RoleConstants.Teacher);

            // Sequential employee number
            var teacherCount = await _unitOfWork.Teachers.CountAsync(t => !t.IsDeleted);

            var teacher = new Teacher
            {
                AppUserId = user.Id,
                EmployeeNumber = EmployeeNumberHelper.Generate(DateTime.Now.Year, teacherCount + 1),
                Title = dto.Title,
                DepartmentId = dto.DepartmentId,
                HireDate = dto.HireDate
            };

            await _unitOfWork.Teachers.AddAsync(teacher);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Create", "Teacher", teacher.Id.ToString(),
                null, new { dto.FullName, dto.Email });
        }

        public async Task UpdateTeacherAsync(UpdateTeacherDto dto)
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(dto.Id);
            if (teacher == null)
                throw new NotFoundException("Müəllim tapılmadı");

            var oldValue = new { teacher.AppUser.FullName, teacher.Title, teacher.DepartmentId };

            teacher.AppUser.FullName = dto.FullName.ToTitleCase();
            teacher.AppUser.PhoneNumber = dto.PhoneNumber;
            await _userManager.UpdateAsync(teacher.AppUser);

            teacher.Title = dto.Title;
            teacher.DepartmentId = dto.DepartmentId;
            teacher.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Teachers.Update(teacher);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Update", "Teacher", dto.Id.ToString(),
                oldValue, new { dto.FullName, dto.Title, dto.DepartmentId });
        }

        public async Task DeleteTeacherAsync(int id)
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(id);
            if (teacher == null)
                throw new NotFoundException("Müəllim tapılmadı");

            teacher.AppUser.IsActive = false;
            await _userManager.UpdateAsync(teacher.AppUser);

            _unitOfWork.Teachers.SoftDelete(teacher);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Delete", "Teacher", id.ToString(),
                new { teacher.AppUser.FullName }, null);
        }
    }
}
