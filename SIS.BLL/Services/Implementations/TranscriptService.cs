using SIS.BLL.Services.DTOs.Transcript;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Helpers;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class TranscriptService : ITranscriptService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TranscriptService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TranscriptDto> GenerateTranscriptAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId)
                ?? throw new NotFoundException("Qeydiyyat tapılmadı");

            var student = enrollment.Student
                ?? throw new NotFoundException("Tələbə tapılmadı");

            var group = enrollment.Group
                ?? throw new NotFoundException("Qrup tapılmadı");

            // ── Assessment Submissions ──
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

            // ── Official Grade: Σ((Score/MaxScore) * WeightPercent) ──
            decimal officialGrade = 0;
            foreach (var a in officialAssessments.Where(a => a.IsGraded && a.MaxScore > 0))
            {
                officialGrade += (a.Score!.Value / a.MaxScore) * a.WeightPercent;
            }

            var letterGrade = GpaHelper.ScoreToLetterGrade(officialGrade);
            var gpa = GpaHelper.CalculateGPA(new[] { (1, officialGrade) });

            // ── Assignment Submissions (ev tapşırıqları) ──
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

            decimal? hwAvg = null;
            var gradedHw = assignments.Where(a => a.IsGraded && a.MaxScore > 0 && !a.IsLinkedToAssessment).ToList();
            if (gradedHw.Any())
                hwAvg = gradedHw.Average(a => (a.Score!.Value / a.MaxScore) * 100);

            // ── Attendance ──
            var attendanceRecords = (await _unitOfWork.AttendanceRecords
                .GetAttendanceByEnrollmentAsync(enrollmentId)).ToList();

            var sessions = await _unitOfWork.ClassSessions.GetByGroupAsync(group.Id);
            var totalSessions = sessions.Count();
            var presentCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Present);
            var lateCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Late);
            var absentCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Absent);
            var attPct = totalSessions > 0 ? (double)(presentCount + lateCount) / totalSessions * 100 : 0;

            // ── Performance & Certificate ──
            var performanceStatus = officialGrade switch
            {
                >= 90 => "Əla (Excellent)",
                >= 80 => "Yaxşı (Good)",
                >= 70 => "Kafi (Satisfactory)",
                >= 60 => "Orta (Average)",
                _ => "Qeyri-kafi (Failing)"
            };

            // Certificate: grade >= 60 və davamiyyət >= 70%
            var certificateEligible = officialGrade >= 60 && attPct >= 70;

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

                TotalSessions = totalSessions,
                PresentCount = presentCount,
                LateCount = lateCount,
                AbsentCount = absentCount,
                AttendancePercentage = Math.Round(attPct, 1),

                OfficialAssessments = officialAssessments,
                OfficialGrade = Math.Round(officialGrade, 2),
                LetterGrade = letterGrade,
                GPA = gpa,

                HomeworkAssignments = assignments,
                HomeworkAverage = hwAvg.HasValue ? Math.Round(hwAvg.Value, 2) : null,

                PerformanceStatus = performanceStatus,
                CertificateEligible = certificateEligible,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
