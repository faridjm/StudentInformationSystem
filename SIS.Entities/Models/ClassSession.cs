using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    public class ClassSession : BaseEntity
    {
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }
        public int SessionNumber { get; set; }

        public ClassSessionStatus Status { get; set; } = ClassSessionStatus.Scheduled;
        public string? Topic { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }

        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<OnlineLesson> OnlineLessons { get; set; } = new List<OnlineLesson>();
    }
}
