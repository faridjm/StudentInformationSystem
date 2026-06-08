using SIS.Entities.Enums;

namespace SIS.BLL.Helpers
{
    /// <summary>
    /// Transcript-in nəticə statusunu (Progress/Final, Completion Level, Certificate Status)
    /// MÖVCUD calculation dəyərlərindən (OfficialGrade, Attendance, çəkilər) törədir.
    /// DİQQƏT: bu helper attendance/grade/GPA hesablamasını DƏYİŞMİR — yalnız onların
    /// üzərində status təsnifatı aparır (AcademicPerformanceService toxunulmur).
    ///
    /// Completion Level bantları (yalnız Final üçün):
    ///   &lt; 65            → NotCompleted
    ///   65 – 84.99       → NormalCompletion
    ///   85 – 94.99       → HonorCompletion
    ///   95 – 100         → HighHonorCompletion
    /// Distinction (Honor / High Honor) YALNIZ eligibility şərtləri ödənəndə verilir:
    ///   OfficialGrade ≥ 65, Attendance ≥ 70, tələb olunan qiymətləndirmələr tamamlanıb,
    ///   kurs/qrup tamamlanıb. Əks halda Certificate = NotEligible və distinction verilmir.
    /// </summary>
    public static class TranscriptResultCalculator
    {
        // Kanonik kodlar — həm DB-də, həm də lokalizasiya açarı kimi istifadə olunur.
        public const string LevelPending = "Pending";
        public const string LevelNotCompleted = "NotCompleted";
        public const string LevelNormal = "NormalCompletion";
        public const string LevelHonor = "HonorCompletion";
        public const string LevelHighHonor = "HighHonorCompletion";
        public const string LevelNotEligible = "NotEligible";

        public const string CertPending = "Pending";
        public const string CertEligible = "Eligible";
        public const string CertNotEligible = "NotEligible";

        public const string StatusCompleted = "Completed";
        public const string StatusInProgress = "InProgress";

        public sealed class Result
        {
            public TranscriptType TranscriptType { get; init; }
            public string CompletionStatus { get; init; } = StatusInProgress;
            public string CompletionLevel { get; init; } = LevelPending;
            public string CertificateStatus { get; init; } = CertPending;
            public bool RequiredAssessmentsCompleted { get; init; }
        }

        /// <param name="officialGrade">AcademicPerformanceService.OfficialGrade (0–100)</param>
        /// <param name="attendancePercentage">AcademicPerformanceService.AttendancePercentage (0–100)</param>
        /// <param name="completedWeight">qiymətləndirilmiş çəkilərin cəmi</param>
        /// <param name="totalWeight">bütün assessment-lərin çəkilərinin cəmi</param>
        /// <param name="courseCompleted">EnrollmentStatus = Completed VƏ YA Group.EndDate keçib</param>
        public static Result Compute(
            decimal officialGrade,
            double attendancePercentage,
            decimal completedWeight,
            decimal totalWeight,
            bool courseCompleted)
        {
            // Tələb olunan qiymətləndirmələr tamamlanıb? (mövcud çəki dəyərlərindən)
            bool requiredCompleted = totalWeight > 0m && completedWeight >= totalWeight - 0.01m;

            var type = courseCompleted ? TranscriptType.Final : TranscriptType.Progress;

            // Kurs davam edir → hər şey Pending / In Progress
            if (type == TranscriptType.Progress)
            {
                return new Result
                {
                    TranscriptType = TranscriptType.Progress,
                    CompletionStatus = StatusInProgress,
                    CompletionLevel = LevelPending,
                    CertificateStatus = CertPending,
                    RequiredAssessmentsCompleted = requiredCompleted
                };
            }

            // ── Final ──
            bool gradeOk = officialGrade >= 65m;
            bool attendanceOk = attendancePercentage >= 70.0;
            bool eligible = gradeOk && attendanceOk && requiredCompleted;

            string level;
            string cert;

            if (!gradeOk)
            {
                level = LevelNotCompleted;
                cert = CertNotEligible;
            }
            else if (eligible)
            {
                cert = CertEligible;
                level = officialGrade >= 95m ? LevelHighHonor
                      : officialGrade >= 85m ? LevelHonor
                      : LevelNormal;
            }
            else
            {
                // Qiymət ≥ 65 amma davamiyyət/tələb olunan qiymətləndirmələr ödənmir
                // → distinction verilmir, sertifikat uyğun deyil.
                cert = CertNotEligible;
                level = LevelNotEligible;
            }

            return new Result
            {
                TranscriptType = TranscriptType.Final,
                CompletionStatus = StatusCompleted,
                CompletionLevel = level,
                CertificateStatus = cert,
                RequiredAssessmentsCompleted = requiredCompleted
            };
        }
    }
}
