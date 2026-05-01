namespace SIS.BLL.Services.DTOs.CourseOffering
{
    public class CreateCourseOfferingDto
    {
        public int CourseId { get; set; }
        public int AcademicTermId { get; set; }
        public string TeacherId { get; set; } = null!;
        public int Capacity { get; set; }
        public string? Room { get; set; }
    }
}
