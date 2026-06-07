using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.DTOs.Performance;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Helpers;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// Bütün attendance/qiymət/GPA hesablamaları üçün vahid mənbə.
    /// Panel və PDF bu serviceə qoşulur — fərqli formula yoxdur.
    /// </summary>
    public class AcademicPerformanceService : IAcademicPerformanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public AcademicPerformanceService(IUnitOfWork unitOfWork, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<AcademicPerformanceDto> GetEnrollmentPerformanceAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId)
                ?? throw new NotFoundException("Qeydiyyat tapılmadı");

            var att = await ComputeAttendanceAsync(enrollmentId, enrollment.GroupId);
            var (officialGrade, completedWeight, totalWeight) = await ComputeOfficialGradeAsync(enrollmentId);
            var hwAvg = await ComputeHomeworkAverageAsync(enrollmentId);

            var letter = GpaHelper.ScoreToLetterGrade(officialGrade);
            var gpa = CalculateGpa(officialGrade);
            var status = ScoreToPerformanceStatus(officialGrade);
            var certEligible = officialGrade >= 60m && att.AttendancePercentage >= 70.0;

            return new AcademicPerformanceDto
            {
                EnrollmentId = enrollment.Id,
                StudentId = enrollment.StudentId,
                GroupId = enrollment.GroupId,

                TotalCompletedSessions = att.TotalCompletedSessions,
                AttendanceTakenSessions = att.AttendanceTakenSessions,
                PresentCount = att.PresentCount,
                LateCount = att.LateCount,
                AbsentCount = att.AbsentCount,
                ExcusedCount = att.ExcusedCount,
                AttendancePercentage = att.AttendancePercentage,

                OfficialGrade = Math.Round(officialGrade, 2),
                CompletedWeight = completedWeight,
                TotalWeight = totalWeight,
                LetterGrade = letter,
                GPA = gpa,

                HomeworkAverage = hwAvg,

                PerformanceStatus = status,
                CertificateEligible = certEligible
            };
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId)
                ?? throw new NotFoundException("Qeydiyyat tapılmadı");
            var att = await ComputeAttendanceAsync(enrollmentId, enrollment.GroupId);
            return new AttendanceSummaryDto
            {
                TotalSessions = att.TotalCompletedSessions,
                TotalCompletedSessions = att.TotalCompletedSessions,
                AttendanceTakenSessions = att.AttendanceTakenSessions,
                PresentCount = att.PresentCount,
                LateCount = att.LateCount,
                AbsentCount = att.AbsentCount,
                ExcusedCount = att.ExcusedCount,
                AttendancePercentage = att.AttendancePercentage
            };
        }

        public decimal CalculateGpa(decimal officialGradePercent)
            => GpaHelper.CalculateGPA(new[] { (1, officialGradePercent) });

        // ── Attendance: tələbə üçün yeganə formula ──
        private async Task<AttendanceBuckets> ComputeAttendanceAsync(int enrollmentId, int groupId)
        {
            var sessions = (await _unitOfWork.ClassSessions.GetByGroupAsync(groupId)).ToList();
            var records = (await _unitOfWork.AttendanceRecords.GetAttendanceByEnrollmentAsync(enrollmentId)).ToList();
            var today = _clock.Today;

            // Yalnız ləğv olunmamış və tarixi keçmiş sessiyalar
            var pastSessions = sessions
                .Where(s => s.Status != ClassSessionStatus.Cancelled && s.Date <= today)
                .ToList();
            var pastSessionIds = pastSessions.Select(s => s.Id).ToHashSet();

            // Davamiyyət götürülmüş sessiya id-ləri (group üzrə hər hansı record varsa)
            var allRecordsByGroup = await _unitOfWork.AttendanceRecords
                .GetAllAsync(a => a.ClassSession != null
                                  && a.ClassSession.GroupId == groupId
                                  && a.ClassSessionId != null
                                  && pastSessionIds.Contains(a.ClassSessionId.Value));
            var takenSessionIds = allRecordsByGroup
                .Select(a => a.ClassSessionId!.Value)
                .ToHashSet();

            var buckets = new AttendanceBuckets
            {
                TotalCompletedSessions = pastSessions.Count,
                AttendanceTakenSessions = takenSessionIds.Count
            };

            // Hər taken session üçün tələbənin record-u yoxdursa → Absent kimi say
            foreach (var sid in takenSessionIds)
            {
                var rec = records.FirstOrDefault(r => r.ClassSessionId == sid);
                if (rec == null)
                {
                    buckets.AbsentCount++;
                    continue;
                }
                switch (rec.Status)
                {
                    case AttendanceStatus.Present: buckets.PresentCount++; break;
                    case AttendanceStatus.Late: buckets.LateCount++; break;
                    case AttendanceStatus.Excused: buckets.ExcusedCount++; break;
                    case AttendanceStatus.Absent: buckets.AbsentCount++; break;
                }
            }

            // Score: Present=1, Excused=1, Late=0.5, Absent=0
            decimal score = buckets.PresentCount + buckets.ExcusedCount + (decimal)buckets.LateCount * 0.5m;
            buckets.AttendancePercentage = buckets.AttendanceTakenSessions > 0
                ? Math.Round((double)(score / buckets.AttendanceTakenSessions * 100m), 2)
                : 0.0;

            return buckets;
        }

        // ── Official grade: yalnız graded AssessmentSubmission-lar ──
        private async Task<(decimal grade, decimal completedWeight, decimal totalWeight)> ComputeOfficialGradeAsync(int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (enrollment == null) return (0m, 0m, 0m);

            var items = (await _unitOfWork.AssessmentItems.GetByGroupAsync(enrollment.GroupId)).ToList();
            var totalWeight = items.Sum(i => i.WeightPercent);

            var subs = (await _unitOfWork.AssessmentSubmissions.GetByEnrollmentAsync(enrollmentId)).ToList();

            decimal grade = 0m;
            decimal completedWeight = 0m;
            foreach (var s in subs.Where(x => x.IsGraded && x.Score.HasValue && x.AssessmentItem != null && x.AssessmentItem.MaxScore > 0))
            {
                var w = s.AssessmentItem!.WeightPercent;
                grade += s.Score!.Value / s.AssessmentItem.MaxScore * w;
                completedWeight += w;
            }
            return (grade, completedWeight, totalWeight);
        }

        // ── Homework average (GPA-ya təsir etmir, AssessmentItem-ə bağlı olmayanlar) ──
        private async Task<decimal?> ComputeHomeworkAverageAsync(int enrollmentId)
        {
            var subs = (await _unitOfWork.AssignmentSubmissions.GetByEnrollmentAsync(enrollmentId)).ToList();
            var graded = subs.Where(s => s.IsGraded && s.Score.HasValue
                                         && s.Assignment != null
                                         && s.Assignment.MaxScore > 0
                                         && s.Assignment.AssessmentItemId == null).ToList();
            if (graded.Count == 0) return null;
            var avg = graded.Average(s => s.Score!.Value / s.Assignment!.MaxScore * 100m);
            return Math.Round(avg, 2);
        }

        private static string ScoreToPerformanceStatus(decimal grade) => grade switch
        {
            >= 90 => "Əla (Excellent)",
            >= 80 => "Yaxşı (Good)",
            >= 70 => "Kafi (Satisfactory)",
            >= 60 => "Orta (Average)",
            _ => "Qeyri-kafi (Failing)"
        };

        private class AttendanceBuckets
        {
            public int TotalCompletedSessions;
            public int AttendanceTakenSessions;
            public int PresentCount;
            public int LateCount;
            public int AbsentCount;
            public int ExcusedCount;
            public double AttendancePercentage;
        }
    }
}
