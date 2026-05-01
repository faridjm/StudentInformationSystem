using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Attendance
{
    public class CreateAttendanceDto
    {
        public int EnrollmentId { get; set; }
        public DateTime SessionDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
    }
}
