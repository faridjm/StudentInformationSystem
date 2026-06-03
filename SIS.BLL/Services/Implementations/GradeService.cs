using SIS.BLL.Services.DTOs.Grade;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IActivityLogger _activityLogger;
        private readonly IClock _clock;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notifications;

        public GradeService(IUnitOfWork unitOfWork, IStudentService studentService,
            IActivityLogger activityLogger, IClock clock, IAuditLogService audit, INotificationService notifications)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _activityLogger = activityLogger;
            _clock = clock;
            _audit = audit;
            _notifications = notifications;
        }

        public async Task<IEnumerable<GradeDto>> GetGradesByEnrollmentAsync(int enrollmentId)
        {
            var grades = await _unitOfWork.Grades.GetGradesByEnrollmentAsync(enrollmentId);
            return grades.Select(MapToDto);
        }

        public async Task<IEnumerable<GradeDto>> GetGradesByStudentAsync(int studentId)
        {
            var grades = await _unitOfWork.Grades.GetGradesByStudentAsync(studentId);
            return grades.Select(MapToDto);
        }

        public async Task AddGradeAsync(CreateGradeDto dto)
        {
            var grade = new Grade
            {
                EnrollmentId = dto.EnrollmentId, Type = dto.Type,
                Score = dto.Score, MaxScore = dto.MaxScore,
                Note = dto.Note, GradedAt = _clock.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Grades.AddAsync(grade);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Create", "Grade", grade.Id.ToString(),
                $"Qiymət əlavə edildi: {grade.Score}/{grade.MaxScore} (Enrollment #{dto.EnrollmentId})");

            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(dto.EnrollmentId);
            if (enrollment != null)
            {
                await _studentService.UpdateGPAAsync(enrollment.StudentId);
                await _notifications.SendNotificationAsync(enrollment.Student?.AppUserId,
                    "Yeni qiymət", $"{grade.Type}: {grade.Score}/{grade.MaxScore}",
                    NotificationType.GradeAdded, "/Student/Grades");
            }
        }

        public async Task UpdateGradeAsync(UpdateGradeDto dto)
        {
            var grade = await _unitOfWork.Grades.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Qiymət tapılmadı");

            grade.Score = dto.Score; grade.MaxScore = dto.MaxScore;
            grade.Note = dto.Note; grade.UpdatedAt = _clock.UtcNow;
            _unitOfWork.Grades.Update(grade);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Update", "Grade", grade.Id.ToString(),
                $"Qiymət yeniləndi: {grade.Score}/{grade.MaxScore} (Enrollment #{grade.EnrollmentId})");

            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(grade.EnrollmentId);
            if (enrollment != null)
                await _studentService.UpdateGPAAsync(enrollment.StudentId);
        }

        public async Task DeleteGradeAsync(int id)
        {
            var grade = await _unitOfWork.Grades.GetByIdAsync(id)
                ?? throw new NotFoundException("Qiymət tapılmadı");
            _unitOfWork.Grades.SoftDelete(grade);
            await _unitOfWork.SaveAsync();
        }

        private static GradeDto MapToDto(Grade g) => new()
        {
            Id = g.Id, EnrollmentId = g.EnrollmentId,
            Type = g.Type, Score = g.Score, MaxScore = g.MaxScore,
            Note = g.Note, GradedAt = g.GradedAt
        };
    }
}
