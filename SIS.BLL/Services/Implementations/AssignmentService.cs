using SIS.BLL.Exceptions;
using SIS.BLL.Services.DTOs.Assignment;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notifications;

        public AssignmentService(IUnitOfWork unitOfWork, IStudentService studentService, IEmailService emailService, IAuditLogService audit, INotificationService notifications)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _emailService = emailService;
            _audit = audit;
            _notifications = notifications;
        }

        public async Task<IEnumerable<AssignmentDto>> GetByGroupAsync(int groupId)
        {
            var assignments = await _unitOfWork.Assignments.GetByGroupAsync(groupId);
            return assignments.Select(MapToDto);
        }

        public async Task<AssignmentDto?> GetByIdAsync(int id)
        {
            var a = await _unitOfWork.Assignments.GetByIdAsync(id);
            return a == null ? null : MapToDto(a);
        }

        public async Task<AssignmentDto> CreateAsync(CreateAssignmentDto dto)
        {
            var assignment = new Assignment
            {
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                DueDate = dto.DueDate,
                MaxScore = dto.MaxScore,
                IsVisible = dto.IsVisible,
                AttachmentUrl = dto.AttachmentUrl,
                GroupId = dto.GroupId,
                AssessmentItemId = dto.AssessmentItemId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Assignments.AddAsync(assignment);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Create", "Assignment", assignment.Id.ToString(),
                $"Tapşırıq yaradıldı: {assignment.Title}");

            // Qrupdakı tələbələrə bildiriş
            var enrolled = await _unitOfWork.Enrollments.GetByGroupAsync(dto.GroupId);
            var studentUserIds = enrolled
                .Where(e => !e.IsDeleted && e.Student != null && e.Student.AppUserId != null)
                .Select(e => e.Student.AppUserId);
            await _notifications.SendToManyAsync(studentUserIds,
                "Yeni tapşırıq", $"\"{assignment.Title}\" tapşırığı əlavə olundu",
                NotificationType.AssignmentCreated, "/Student/Assignments");

            return MapToDto(assignment);
        }

        public async Task UpdateAsync(UpdateAssignmentDto dto)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(dto.Id)
                ?? throw new AppException.NotFoundException("Tapşırıq tapılmadı");

            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.Type = dto.Type;
            assignment.DueDate = dto.DueDate;
            assignment.MaxScore = dto.MaxScore;
            assignment.IsVisible = dto.IsVisible;
            assignment.AttachmentUrl = dto.AttachmentUrl;

            _unitOfWork.Assignments.Update(assignment);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(id)
                ?? throw new AppException.NotFoundException("Tapşırıq tapılmadı");

            _unitOfWork.Assignments.SoftDelete(assignment);
            await _unitOfWork.SaveAsync();
        }

        // ── Submissions ──────────────────────────────────────────────

        public async Task<IEnumerable<SubmissionDto>> GetSubmissionsAsync(int assignmentId)
        {
            var subs = await _unitOfWork.AssignmentSubmissions.GetByAssignmentAsync(assignmentId);
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            return subs.Select(s => MapSubmissionToDto(s, assignment?.MaxScore ?? 0));
        }

        public async Task<IEnumerable<SubmissionDto>> GetMySubmissionsAsync(int enrollmentId)
        {
            var subs = await _unitOfWork.AssignmentSubmissions.GetByEnrollmentAsync(enrollmentId);
            return subs.Select(s => MapSubmissionToDto(s, s.Assignment?.MaxScore ?? 0));
        }

        public async Task<SubmissionDto?> GetSubmissionAsync(int assignmentId, int enrollmentId)
        {
            var sub = await _unitOfWork.AssignmentSubmissions.GetByAssignmentAndEnrollmentAsync(assignmentId, enrollmentId);
            if (sub == null) return null;
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            return MapSubmissionToDto(sub, assignment?.MaxScore ?? 0);
        }

        public async Task<SubmissionDto> SubmitAsync(CreateSubmissionDto dto)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(dto.AssignmentId)
                ?? throw new AppException.NotFoundException("Tapşırıq tapılmadı");

            var existing = await _unitOfWork.AssignmentSubmissions
                .GetByAssignmentAndEnrollmentAsync(dto.AssignmentId, dto.EnrollmentId);

            if (existing != null)
            {
                // Yenidən göndərə bilər — mövcud cavabı yeniləyir
                existing.Comment = dto.Comment;
                existing.FileUrl = dto.FileUrl;
                existing.SubmittedAt = DateTime.UtcNow;
                existing.IsGraded = false;
                existing.Score = null;
                existing.Feedback = null;
                _unitOfWork.AssignmentSubmissions.Update(existing);
                await _unitOfWork.SaveAsync();
                await _audit.LogAsync("Submit", "Assignment", assignment.Id.ToString(),
                    $"Tapşırıq cavabı yeniləndi (Enrollment #{dto.EnrollmentId})");
                await NotifyTeacherOnSubmitAsync(assignment);
                return MapSubmissionToDto(existing, assignment.MaxScore);
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = dto.AssignmentId,
                EnrollmentId = dto.EnrollmentId,
                Comment = dto.Comment,
                FileUrl = dto.FileUrl,
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AssignmentSubmissions.AddAsync(submission);
            await _unitOfWork.SaveAsync();
            await _audit.LogAsync("Submit", "Assignment", assignment.Id.ToString(),
                $"Tapşırıq cavabı göndərildi (Enrollment #{dto.EnrollmentId})");
            await NotifyTeacherOnSubmitAsync(assignment);
            return MapSubmissionToDto(submission, assignment.MaxScore);
        }

        public async Task GradeAsync(GradeSubmissionDto dto)
        {
            var submission = await _unitOfWork.AssignmentSubmissions.GetByIdAsync(dto.SubmissionId)
                ?? throw new AppException.NotFoundException("Cavab tapılmadı");

            var assignment = await _unitOfWork.Assignments.GetByIdAsync(submission.AssignmentId)
                ?? throw new AppException.NotFoundException("Tapşırıq tapılmadı");

            if (dto.Score > assignment.MaxScore)
                throw new AppException.BadRequestException($"Qiymət maksimum ({assignment.MaxScore}) keçə bilməz");

            submission.Score = dto.Score;
            submission.Feedback = dto.Feedback;
            submission.IsGraded = true;
            _unitOfWork.AssignmentSubmissions.Update(submission);

            // ── Əgər assignment rəsmi AssessmentItem-ə bağlıdırsa → AssessmentSubmission-a da yaz ──
            if (assignment.AssessmentItemId.HasValue)
            {
                var assessmentSub = (await _unitOfWork.AssessmentSubmissions
                    .GetByEnrollmentAsync(submission.EnrollmentId))
                    .FirstOrDefault(s => s.AssessmentItemId == assignment.AssessmentItemId.Value);

                if (assessmentSub != null)
                {
                    assessmentSub.Score = dto.Score;
                    assessmentSub.Feedback = dto.Feedback;
                    assessmentSub.IsGraded = true;
                    assessmentSub.GradedAt = DateTime.UtcNow;
                    _unitOfWork.AssessmentSubmissions.Update(assessmentSub);
                }
            }

            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Review", "Assignment", assignment.Id.ToString(),
                $"Tapşırıq qiymətləndirildi: {dto.Score}/{assignment.MaxScore} (Submission #{dto.SubmissionId})");

            // GPA-nı yenilə + email göndər
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(submission.EnrollmentId);
            if (enrollment != null)
            {
                await _studentService.UpdateGPAAsync(enrollment.StudentId);

                await _notifications.SendNotificationAsync(enrollment.Student?.AppUserId,
                    "Tapşırıq qiymətləndirildi", $"\"{assignment.Title}\": {dto.Score}/{assignment.MaxScore}",
                    NotificationType.AssignmentReviewed, "/Student/Assignments");

                var studentEmail = enrollment.Student?.AppUser?.Email;
                var studentName = enrollment.Student?.AppUser?.FullName ?? "";
                var courseName = enrollment.Group?.Course?.Name ?? "";
                if (!string.IsNullOrEmpty(studentEmail))
                    _ = _emailService.SendGradeNotificationAsync(studentEmail, studentName, assignment.Title, courseName, dto.Score, assignment.MaxScore, dto.Feedback);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────

        private async Task NotifyTeacherOnSubmitAsync(Assignment assignment)
        {
            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(assignment.GroupId);
            var teacherUserId = group?.Teacher?.AppUserId;
            await _notifications.SendNotificationAsync(teacherUserId,
                "Tapşırıq cavabı", $"\"{assignment.Title}\" tapşırığına yeni cavab göndərildi",
                NotificationType.AssignmentSubmitted, $"/Teacher/Assignments/Submissions/{assignment.Id}");
        }

        private static AssignmentDto MapToDto(Assignment a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            Type = a.Type,
            DueDate = a.DueDate,
            MaxScore = a.MaxScore,
            IsVisible = a.IsVisible,
            AttachmentUrl = a.AttachmentUrl,
            GroupId = a.GroupId,
            SubmissionCount = a.Submissions?.Count ?? 0,
            AssessmentItemId = a.AssessmentItemId,
            AssessmentItemTitle = a.AssessmentItem?.Title
        };

        private static SubmissionDto MapSubmissionToDto(AssignmentSubmission s, decimal maxScore) => new()
        {
            Id = s.Id,
            AssignmentId = s.AssignmentId,
            AssignmentTitle = s.Assignment?.Title ?? "",
            EnrollmentId = s.EnrollmentId,
            StudentName = s.Enrollment?.Student?.AppUser?.FullName ?? "",
            StudentNumber = s.Enrollment?.Student?.StudentNumber ?? "",
            Comment = s.Comment,
            FileUrl = s.FileUrl,
            SubmittedAt = s.SubmittedAt,
            Score = s.Score,
            Feedback = s.Feedback,
            IsGraded = s.IsGraded,
            MaxScore = maxScore
        };
    }
}
