using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SIS.BLL.Services.DTOs.Student;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Extensions;
using SIS.Common.Helpers;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Identity;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IEnumerable<StudentListDto>> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.Students.GetStudentsWithGPAAsync();
            return _mapper.Map<IEnumerable<StudentListDto>>(students);
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
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, SIS.Common.Constants.RoleConstants.Student);

            var student = new Student
            {
                AppUserId = user.Id,
                StudentNumber = StudentNumberHelper.Generate(dto.EnrollmentYear),
                EnrollmentYear = dto.EnrollmentYear,
                AcademicProgramId = dto.AcademicProgramId
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateStudentAsync(UpdateStudentDto dto)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(dto.Id);
            if (student == null)
                throw new Exception("Tələbə tapılmadı");

            student.AppUser.FullName = dto.FullName.ToTitleCase();
            student.AppUser.PhoneNumber = dto.PhoneNumber;
            await _userManager.UpdateAsync(student.AppUser);

            student.AcademicProgramId = dto.AcademicProgramId;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            if (student == null)
                throw new Exception("Tələbə tapılmadı");

            student.AppUser.IsActive = false;
            await _userManager.UpdateAsync(student.AppUser);

            _unitOfWork.Students.SoftDelete(student);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateGPAAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
            if (student == null) return;

            var creditScoreMap = new Dictionary<int, decimal>();

            foreach (var enrollment in student.Enrollments
                .Where(e => e.Status == SIS.Entities.Enums.EnrollmentStatus.Active
                         || e.Status == SIS.Entities.Enums.EnrollmentStatus.Completed))
            {
                var avgScore = await _unitOfWork.Grades.GetAverageScoreAsync(enrollment.Id);
                if (avgScore > 0)
                {
                    var credits = enrollment.CourseOffering.Course.Credits;
                    creditScoreMap[credits] = avgScore;
                }
            }

            student.CurrentGPA = GpaHelper.CalculateGPA(creditScoreMap);
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveAsync();
        }
    }
}
