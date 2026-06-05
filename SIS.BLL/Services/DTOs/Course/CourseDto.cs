namespace SIS.BLL.Services.DTOs.Course
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int GroupCount { get; set; }
        public int Credit { get; set; }
        public int TotalHours { get; set; }
        public string? Level { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class CreateCourseDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Credit { get; set; }
        public int TotalHours { get; set; }
        public string? Level { get; set; }
        public int? DepartmentId { get; set; }
    }

    public class UpdateCourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int Credit { get; set; }
        public int TotalHours { get; set; }
        public string? Level { get; set; }
        public int? DepartmentId { get; set; }
    }
}
