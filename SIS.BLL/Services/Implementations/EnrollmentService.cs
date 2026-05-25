using AutoMapper;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IActivityLogger _activityLogger;

        public EnrollmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IActivityLogger activityLogger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _activityLogger = activityLogger;
        }

        public async Task<IEnumerable<EnrollmentListDto>> GetAllEnrollmentsAsync()
        {
            var enrollments = await _unitOfWork.Enrollments.GetAllAsync();
            return _mapper.Map<IEnumerable<EnrollmentListDto>>(enrollments);
        }

        public async Task<EnrollmentDetailDto?> GetEnrollmentByIdAsync(int id)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(id);
            if (enrollment == null) return null;

            var dto = _mapper.Map<EnrollmentDetailDto>(enrollment);
            dto.AverageScore = await _unitOfWork.Grades.GetAverageScoreAsync(id);
            dto.AttendancePercentage = await _unitOfWork.AttendanceRecords.GetAttendancePercentageAsync(id);
            return dto;
        }

        public async Task<IEnumerable<EnrollmentListDto>> GetStudentEnrollmentsAsync(int studentId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetStudentEnrollmentsAsync(studentId);
            return _mapper.Map<IEnumerable<EnrollmentListDto>>(enrollments);
        }

        public async Task<IEnumerable<EnrollmentListDto>> GetCourseEnrollmentsAsync(int courseOfferingId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetCourseEnrollmentsAsync(courseOfferingId);
            return enrollments.Select(e => new EnrollmentListDto
            {
                Id = e.Id,
                StudentName = e.Student.AppUser.FullName,
                StudentNumber = e.Student.StudentNumber,
                CourseName = e.CourseOffering.Course.Name,
                CourseCode = e.CourseOffering.Course.Code,
                TeacherName = e.CourseOffering.Teacher.AppUser.FullName,
                TermName = e.CourseOffering.AcademicTerm.Name,
                Status = e.Status,
                EnrolledAt = e.EnrolledAt
            }).ToList();
        }

        public async Task EnrollStudentAsync(CreateEnrollmentDto dto)
        {
            var isEnrolled = await _unitOfWork.Enrollments
                .IsStudentEnrolledAsync(dto.StudentId, dto.CourseOfferingId);
            if (isEnrolled)
                throw new ConflictException("Tələbə artıq bu kursa qeydiyyatlıdır");

            var hasCapacity = await _unitOfWork.CourseOfferings
                .HasCapacityAsync(dto.CourseOfferingId);
            if (!hasCapacity)
                throw new BadRequestException("Kursda boş yer yoxdur");

            var enrollment = _mapper.Map<Enrollment>(dto);
            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Create", "Enrollment", enrollment.Id.ToString(),
                null, new { dto.StudentId, dto.CourseOfferingId });
        }

        public async Task DropEnrollmentAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (enrollment == null)
                throw new NotFoundException("Qeydiyyat tapılmadı");

            enrollment.Status = SIS.Entities.Enums.EnrollmentStatus.Dropped;
            enrollment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Enrollments.Update(enrollment);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Update", "Enrollment", enrollmentId.ToString(),
                new { Status = "Active" }, new { Status = "Dropped" });
        }
    }
}
