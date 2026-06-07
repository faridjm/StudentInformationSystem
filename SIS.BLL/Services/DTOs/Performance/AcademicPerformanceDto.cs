namespace SIS.BLL.Services.DTOs.Performance
{
    /// <summary>
    /// Bir tələbənin bir qrupdakı tam akademik göstəriciləri.
    /// Panel və PDF eyni mənbədən bu DTO-nu istifadə edir.
    /// </summary>
    public class AcademicPerformanceDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int GroupId { get; set; }

        // Attendance
        public int TotalCompletedSessions { get; set; }   // ləğv olunmamış, tarixi keçən sessiyalar
        public int AttendanceTakenSessions { get; set; }  // ən azı 1 record olan sessiyalar
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendancePercentage { get; set; }  // (Present + Excused + 0.5*Late) / AttendanceTakenSessions * 100

        // Grade
        public decimal OfficialGrade { get; set; }        // Σ((Score/MaxScore) * WeightPercent)
        public decimal CompletedWeight { get; set; }      // qiymətləndirilmiş çəkilərin cəmi
        public decimal TotalWeight { get; set; }          // bütün assessment-lərin çəkiləri
        public string LetterGrade { get; set; } = "F";
        public decimal GPA { get; set; }                  // 0.0 — 4.0

        // Homework (GPA-ya təsir etmir)
        public decimal? HomeworkAverage { get; set; }

        // Status
        public string PerformanceStatus { get; set; } = "";
        public bool CertificateEligible { get; set; }
    }
}
