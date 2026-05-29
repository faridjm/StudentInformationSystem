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

        public AssignmentService(IUnitOfWork unitOfWork, IStudentService studentService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<AssignmentDto>> GetByCourseOfferingAsync(int courseOfferingId)
        {
            var assignments = await _unitOfWork.Assignments.GetByCourseOfferingAsync(courseOfferingId);
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
                CourseOfferingId = dto.CourseOfferingId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Assignments.AddAsync(assignment);
            await _unitOfWork.SaveAsync();
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

            // Grade cədvəlinə də yaz
            var gradeType = assignment.Type switch
            {
                AssignmentType.Quiz => GradeType.Quiz,
                AssignmentType.Project => GradeType.Project,
                _ => GradeType.Assignment
            };

            var existingGrade = await _unitOfWork.Grades.GetAsync(g =>
                g.EnrollmentId == submission.EnrollmentId &&
                g.Type == gradeType);

            if (existingGrade != null)
            {
                existingGrade.Score = dto.Score;
                existingGrade.MaxScore = assignment.MaxScore;
                existingGrade.Note = $"{assignment.Title} — {dto.Feedback}";
                _unitOfWork.Grades.Update(existingGrade);
            }
            else
            {
                await _unitOfWork.Grades.AddAsync(new Grade
                {
                    EnrollmentId = submission.EnrollmentId,
                    Type = gradeType,
                    Score = dto.Score,
                    MaxScore = assignment.MaxScore,
                    Note = $"{assignment.Title} — {dto.Feedback}",
                    GradedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveAsync();

            // GPA-nı yenilə + email göndər
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(submission.EnrollmentId);
            if (enrollment != null)
            {
                await _studentService.UpdateGPAAsync(enrollment.StudentId);

                var studentEmail = enrollment.Student?.AppUser?.Email;
                var studentName = enrollment.Student?.AppUser?.FullName ?? "";
                var courseName = enrollment.CourseOffering?.Course?.Name ?? "";
                if (!string.IsNullOrEmpty(studentEmail))
                    _ = _emailService.SendGradeNotificationAsync(studentEmail, studentName, assignment.Title, courseName, dto.Score, assignment.MaxScore, dto.Feedback);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────

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
            CourseOfferingId = a.CourseOfferingId,
            SubmissionCount = a.Submissions?.Count ?? 0
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
