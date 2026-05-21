namespace SIS.BLL.Services.DTOs.CourseOffering
{
    public class CourseOfferingDetailDto
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public int Credits { get; set; }
        public string TeacherName { get; set; } = null!;
        public string TermName { get; set; } = null!;
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public string? Room { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TeacherId { get; set; }
    }
}
