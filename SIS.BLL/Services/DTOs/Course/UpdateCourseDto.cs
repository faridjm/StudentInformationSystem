namespace SIS.BLL.Services.DTOs.Course
{
    public class UpdateCourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public int AcademicProgramId { get; set; }
    }
}
