using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    public class Enrollment : BaseEntity
    {
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
        public int StudentId { get; set; }
        public int CourseOfferingId { get; set; }
        public Student Student { get; set; } = null!;
        public CourseOffering CourseOffering { get; set; } = null!;
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    }
}
