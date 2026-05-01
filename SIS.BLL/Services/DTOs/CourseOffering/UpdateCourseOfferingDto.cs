namespace SIS.BLL.Services.DTOs.CourseOffering
{
    public class UpdateCourseOfferingDto
    {
        public int Id { get; set; }
        public string TeacherId { get; set; } = null!;
        public int Capacity { get; set; }
        public string? Room { get; set; }
    }
}
