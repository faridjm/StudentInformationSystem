namespace SIS.BLL.Services.DTOs.CourseOffering
{
    public class CreateCourseOfferingDto
    {
        public int CourseId { get; set; }
        public int AcademicTermId { get; set; }
        public int TeacherId { get; set; }
        public int Capacity { get; set; }
        public string? Room { get; set; }
    }
}
