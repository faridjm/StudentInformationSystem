using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    public class AttendanceRecord : BaseEntity
    {
        public DateTime SessionDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;
    }
}
