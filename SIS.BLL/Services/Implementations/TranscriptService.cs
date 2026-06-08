using SIS.BLL.Services.DTOs.Transcript;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Helpers;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// Transkript DTO-su üçün məlumatları toplayır. Attendance, official grade,
    /// GPA və homework average — hamısı IAcademicPerformanceService-dən gəlir,
    /// yəni panel və PDF eyni nəticəni göstərir.
    /// </summary>
    public class TranscriptService : ITranscriptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAcademicPerformanceService _performance;

        public TranscriptService(IUnitOfWork unitOfWork, IAcademicPerformanceService performance)
        {
            _unitOfWork = unitOfWork;
            _performance = performance;
        }

        public async Task<TranscriptDto> GenerateTranscriptAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId)
                ?? throw new NotFoundException("Qeydiyyat tapılmadı");

            var student = enrollment.Student
                ?? throw new NotFoundException("Tələbə tapılmadı");
            var group = enrollment.Group
                ?? throw new NotFoundException("Qrup tapılmadı");

            // ── Vahid mənbədən performans ──
            var perf = await _performance.GetEnrollmentPerformanceAsync(enrollmentId);

            // Assessment cədvəli üçün submission-ları detallı göstər
            var assessmentSubs = (await _unitOfWork.AssessmentSubmissions
                .GetByEnrollmentAsync(enrollmentId)).ToList();

            var officialAssessments = assessmentSubs.Select(s => new TranscriptAssessmentDto
            {
                Title = s.AssessmentItem?.Title ?? "",
                Type = s.AssessmentItem?.Type switch
                {
                    AssessmentType.Quiz => "Quiz",
                    AssessmentType.FinalProject => "Final Layihəsi",
                    AssessmentType.Assignment => "Tapşırıq",
                    AssessmentType.Exam => "İmtahan",
                    AssessmentType.Homework => "Ev tapşırığı",
                    _ => "Digər"
                },
                Date = s.AssessmentItem?.Date ?? DateTime.MinValue,
                MaxScore = s.AssessmentItem?.MaxScore ?? 0,
                WeightPercent = s.AssessmentItem?.WeightPercent ?? 0,
                Score = s.Score,
                IsGraded = s.IsGraded
            }).OrderBy(a => a.Date).ToList();

            // Homework siyahısı (transkript-də ayrıca göstərilir)
            var assignmentSubs = (await _unitOfWork.AssignmentSubmissions
                .GetByEnrollmentAsync(enrollmentId)).ToList();

            var assignments = assignmentSubs.Select(s => new TranscriptAssignmentDto
            {
                Title = s.Assignment?.Title ?? "",
                DueDate = s.Assignment?.DueDate ?? DateTime.MinValue,
                MaxScore = s.Assignment?.MaxScore ?? 0,
                Score = s.Score,
                IsGraded = s.IsGraded,
                IsSubmitted = true,
                IsLinkedToAssessment = s.Assignment?.AssessmentItemId != null
            }).OrderBy(a => a.DueDate).ToList();

            // ── Progress / Final + Completion Level (mövcud dəyərlərdən törədilir) ──
            // Kurs bitib sayılır: EnrollmentStatus = Completed VƏ YA Group.EndDate keçib.
            bool courseCompleted = enrollment.Status == EnrollmentStatus.Completed
                                   || group.EndDate.Date < DateTime.UtcNow.Date;

            var result = SIS.BLL.Helpers.TranscriptResultCalculator.Compute(
                perf.OfficialGrade,
                perf.AttendancePercentage,
                perf.CompletedWeight,
                perf.TotalWeight,
                courseCompleted);

            return new TranscriptDto
            {
                StudentName = student.AppUser?.FullName ?? "",
                StudentNumber = student.StudentNumber,
                Email = student.AppUser?.Email,
                Phone = student.AppUser?.PhoneNumber,
                EnrollmentYear = student.EnrollmentYear,

                CourseName = group.Course?.Name ?? "",
                GroupName = group.Name,
                TeacherName = group.Teacher?.AppUser?.FullName ?? "",
                AcademicYear = group.AcademicYear?.Name,
                StartDate = group.StartDate,
                EndDate = group.EndDate,

                TotalSessions = perf.TotalCompletedSessions,
                AttendanceTakenSessions = perf.AttendanceTakenSessions,
                PresentCount = perf.PresentCount,
                LateCount = perf.LateCount,
                AbsentCount = perf.AbsentCount,
                ExcusedCount = perf.ExcusedCount,
                AttendancePercentage = perf.AttendancePercentage,

                OfficialAssessments = officialAssessments,
                OfficialGrade = perf.OfficialGrade,
                LetterGrade = perf.LetterGrade,
                GPA = perf.GPA,

                HomeworkAssignments = assignments,
                HomeworkAverage = perf.HomeworkAverage,

                PerformanceStatus = perf.PerformanceStatus,
                CertificateEligible = perf.CertificateEligible,
                GeneratedAt = DateTime.UtcNow,

                EnrollmentId = enrollment.Id,
                StudentId = enrollment.StudentId,
                GroupId = enrollment.GroupId,

                IsCourseCompleted = courseCompleted,
                TranscriptType = result.TranscriptType,
                CompletionStatus = result.CompletionStatus,
                CompletionLevel = result.CompletionLevel,
                CertificateStatus = result.CertificateStatus
            };
        }
    }
}
