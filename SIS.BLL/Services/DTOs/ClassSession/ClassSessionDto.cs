using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.ClassSession
{
    public class ClassSessionDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SessionNumber { get; set; }
        public DateOnly Date { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public string? Room { get; set; }
        public string? Topic { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public string CourseName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public string CourseCode { get; set; } = "";
        public ClassSessionStatus Status { get; set; }
    }
}
