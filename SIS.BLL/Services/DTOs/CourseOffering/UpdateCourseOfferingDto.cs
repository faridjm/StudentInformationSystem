namespace SIS.BLL.Services.DTOs.CourseOffering
{
    public class UpdateCourseOfferingDto
    {
        public int Id { get; set; }
        public int TeacherId { get; set; } 
        public int Capacity { get; set; }
        public string? Room { get; set; }
    }
}
