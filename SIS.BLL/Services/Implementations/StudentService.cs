using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SIS.BLL.Services.DTOs.Student;
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
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IActivityLogger _activityLogger;

        public StudentService(
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

        public async Task<PagedResult<StudentListDto>> GetAllStudentsAsync(PagedRequest request)
        {
            var pagedStudents = await _unitOfWork.Students.GetPagedAsync(request);

            return new PagedResult<StudentListDto>
            {
                Items = _mapper.Map<IEnumerable<StudentListDto>>(pagedStudents.Items),
                TotalCount = pagedStudents.TotalCount,
                Page = pagedStudents.Page,
                PageSize = pagedStudents.PageSize
            };
        }

        public async Task<StudentDetailDto?> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            return _mapper.Map<StudentDetailDto>(student);
        }

        public async Task<StudentDetailDto?> GetStudentByUserIdAsync(string userId)
        {
            var student = await _unitOfWork.Students.GetStudentByUserIdAsync(userId);
            return _mapper.Map<StudentDetailDto>(student);
        }

        public async Task<IEnumerable<StudentListDto>> GetStudentsByProgramAsync(int programId)
        {
            var students = await _unitOfWork.Students.GetStudentsByProgramAsync(programId);
            return _mapper.Map<IEnumerable<StudentListDto>>(students);
        }

        public async Task CreateStudentAsync(CreateStudentDto dto)
        {
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                var phoneExists = await _userManager.Users
                    .AnyAsync(u => u.PhoneNumber == dto.PhoneNumber);
                if (phoneExists)
                    throw new ConflictException("Bu telefon nömrəsi artıq istifadədədir");
            }

            if (!string.IsNullOrEmpty(dto.FinCode))
            {
                var finExists = await _userManager.Users
                    .AnyAsync(u => u.FinCode == dto.FinCode);
                if (finExists)
                    throw new ConflictException("Bu FIN kod artıq istifadədədir");
            }

            var user = new AppUser
            {
                FullName = dto.FullName.ToTitleCase(),
                FinCode = dto.FinCode,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, RoleConstants.Student);

            // Sequential student number
            var studentCount = await _unitOfWork.Students.CountAsync(s => !s.IsDeleted);

            var student = new Student
            {
                AppUserId = user.Id,
                StudentNumber = StudentNumberHelper.Generate(dto.EnrollmentYear, studentCount + 1),
                EnrollmentYear = dto.EnrollmentYear,
                AcademicProgramId = dto.AcademicProgramId
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Create", "Student", student.Id.ToString(),
                null, new { dto.FullName, dto.Email });
        }

        public async Task UpdateStudentAsync(UpdateStudentDto dto)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(dto.Id);
            if (student == null)
                throw new NotFoundException("Tələbə tapılmadı");

            var oldValue = new { student.AppUser.FullName, student.AcademicProgramId };

            student.AppUser.FullName = dto.FullName.ToTitleCase();
            student.AppUser.FinCode = dto.FinCode;
            student.AppUser.PhoneNumber = dto.PhoneNumber;
            await _userManager.UpdateAsync(student.AppUser);

            student.AcademicProgramId = dto.AcademicProgramId;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Update", "Student", dto.Id.ToString(),
                oldValue, new { dto.FullName, dto.AcademicProgramId });
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            if (student == null)
                throw new NotFoundException("Tələbə tapılmadı");

            student.AppUser.Email = $"deleted_{id}_{student.AppUser.Email}";
            student.AppUser.NormalizedEmail = student.AppUser.Email.ToUpper();
            student.AppUser.UserName = student.AppUser.Email;
            student.AppUser.NormalizedUserName = student.AppUser.Email.ToUpper();
            student.AppUser.IsActive = false;
            await _userManager.UpdateAsync(student.AppUser);

            _unitOfWork.Students.SoftDelete(student);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Delete", "Student", id.ToString(),
                new { student.AppUser.FullName }, null);
        }

        public async Task UpdateGPAAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
            if (student == null) return;

            var items = new List<(int credits, decimal score)>();

            foreach (var enrollment in student.Enrollments
                .Where(e => e.Status == SIS.Entities.Enums.EnrollmentStatus.Active
                         || e.Status == SIS.Entities.Enums.EnrollmentStatus.Completed))
            {
                var avgScore = await _unitOfWork.Grades.GetAverageScoreAsync(enrollment.Id);
                if (avgScore > 0)
                {
                    var credits = enrollment.CourseOffering.Course.Credits;
                    items.Add((credits, avgScore));
                }
            }

            student.CurrentGPA = GpaHelper.CalculateGPA(items);
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveAsync();
        }
    }
}
