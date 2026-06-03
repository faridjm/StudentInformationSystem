using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;
// AssessmentSubmission is in SIS.Entities.Models

namespace SIS.BLL.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _audit;

        public EnrollmentService(IUnitOfWork unitOfWork, IEmailService emailService, IAuditLogService audit)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _audit = audit;
        }

        public async Task<IEnumerable<EnrollmentListDto>> GetByGroupAsync(int groupId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetByGroupAsync(groupId);
            return enrollments.Select(MapToDto);
        }

        public async Task<IEnumerable<EnrollmentListDto>> GetByStudentAsync(int studentId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetByStudentAsync(studentId);
            return enrollments.Select(MapToDto);
        }

        public async Task EnrollAsync(CreateEnrollmentDto dto)
        {
            if (await _unitOfWork.Enrollments.IsStudentEnrolledAsync(dto.StudentId, dto.GroupId))
                throw new ConflictException("Tələbə artıq bu qrupa qeydiyyatlıdır");

            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(dto.GroupId)
                ?? throw new NotFoundException("Qrup tapılmadı");

            if (group.Enrollments.Count(e => !e.IsDeleted) >= group.Capacity)
                throw new BadRequestException("Qrupda boş yer yoxdur");

            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                GroupId = dto.GroupId,
                EnrolledAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.SaveAsync();

            // Bu qrupun bütün assessment item-ləri üçün submission yarat
            var assessmentItems = await _unitOfWork.AssessmentItems.GetByGroupAsync(dto.GroupId);
            foreach (var item in assessmentItems)
            {
                await _unitOfWork.AssessmentSubmissions.AddAsync(new AssessmentSubmission
                {
                    AssessmentItemId = item.Id,
                    EnrollmentId = enrollment.Id,
                    IsGraded = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (assessmentItems.Any())
                await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Enroll", "Enrollment", enrollment.Id.ToString(),
                $"Tələbə qrupa qeydiyyatdan keçdi (Student #{dto.StudentId}, Group #{dto.GroupId})");

            // Email göndər
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(dto.StudentId);
            if (student?.AppUser?.Email != null)
            {
                var teacherName = group.Teacher?.AppUser?.FullName ?? "";
                _ = _emailService.SendEnrollmentConfirmationAsync(
                    student.AppUser.Email, student.AppUser.FullName,
                    group.Course?.Name ?? "", teacherName, group.AcademicYear?.Name ?? "");
            }
        }

        public async Task<EnrollmentListDto?> GetByIdAsync(int enrollmentId)
        {
            var e = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);
            return e == null ? null : MapToDto(e);
        }

        public async Task DropAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId)
                ?? throw new NotFoundException("Qeydiyyat tapılmadı");

            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Enrollments.Update(enrollment);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Drop", "Enrollment", enrollment.Id.ToString(),
                $"Qeydiyyat ləğv edildi (Student #{enrollment.StudentId}, Group #{enrollment.GroupId})");
        }

        private static EnrollmentListDto MapToDto(Enrollment e) => new()
        {
            Id = e.Id,
            StudentId = e.StudentId,
            StudentName = e.Student?.AppUser?.FullName ?? "",
            StudentNumber = e.Student?.StudentNumber ?? "",
            GroupId = e.GroupId,
            GroupName = e.Group?.Name ?? "",
            CourseName = e.Group?.Course?.Name ?? "",
            TeacherId = e.Group?.TeacherId ?? 0,
            TeacherName = e.Group?.Teacher?.AppUser?.FullName ?? "",
            Status = e.Status,
            EnrolledAt = e.EnrolledAt
        };
    }
}
