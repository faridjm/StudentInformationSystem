using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Attendance
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public DateTime SessionDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public int? ClassSessionId { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
    }

    public class CreateAttendanceDto
    {
        public int EnrollmentId { get; set; }
        public int? ClassSessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
    }
}
