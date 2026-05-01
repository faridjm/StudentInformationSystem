namespace SIS.BLL.Services.DTOs.Course
{
    public class CourseDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public string DepartmentName { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
