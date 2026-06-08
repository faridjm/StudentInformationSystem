namespace SIS.BLL.Services.DTOs.Performance
{
    /// <summary>
    /// Group detail səhifəsində bir cədvəldə tələbə-tələbə performans göstəriciləri.
    /// AcademicPerformanceService-dən gəlir — panel və PDF ilə eyni hesablama.
    /// </summary>
    public class GroupStudentPerformanceDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public string? StudentEmail { get; set; }

        // Attendance
        public int TotalCompletedSessions { get; set; }
        public int AttendanceTakenSessions { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendancePercentage { get; set; }

        // Grade
        public decimal OfficialGrade { get; set; }
        public decimal CompletedWeight { get; set; }
        public decimal TotalWeight { get; set; }
        public string LetterGrade { get; set; } = "F";
        public decimal GPA { get; set; }

        public string PerformanceStatus { get; set; } = "";
        public bool CertificateEligible { get; set; }
    }
}
