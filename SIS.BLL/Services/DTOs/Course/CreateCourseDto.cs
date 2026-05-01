namespace SIS.BLL.Services.DTOs.Course
{
    public class CreateCourseDto
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
    }
}
