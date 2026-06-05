using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    public class Group : BaseEntity
    {
        public string Name { get; set; } = null!;
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
        public int? AcademicYearId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;

        // Tədris formatı
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.InPerson;
        public OnlinePlatform? DefaultOnlinePlatform { get; set; }
        public string? DefaultMeetingUrl { get; set; }

        public Course Course { get; set; } = null!;
        public Teacher Teacher { get; set; } = null!;
        public AcademicYear? AcademicYear { get; set; }

        public ICollection<GroupScheduleDay> ScheduleDays { get; set; } = new List<GroupScheduleDay>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();
        public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<OnlineLesson> OnlineLessons { get; set; } = new List<OnlineLesson>();
    }
}
