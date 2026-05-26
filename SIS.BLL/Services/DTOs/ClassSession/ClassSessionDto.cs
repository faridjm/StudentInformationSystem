using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.ClassSession
{
    public class ClassSessionDto
    {
        public int Id { get; set; }
        public int CourseOfferingId { get; set; }
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string? TeacherName { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }

        public ClassSessionStatus Status { get; set; }
        public string StatusName { get; set; } = null!;

        public string? Topic { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }

        public int? OriginalSessionId { get; set; }
    }
}
