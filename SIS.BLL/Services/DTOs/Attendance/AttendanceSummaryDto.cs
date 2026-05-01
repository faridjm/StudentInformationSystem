namespace SIS.BLL.Services.DTOs.Attendance
{
    public class AttendanceSummaryDto
    {
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
    }
}
