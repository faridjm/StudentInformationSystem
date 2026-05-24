namespace SIS.BLL.Services.DTOs.Teacher
{
    public class UpdateTeacherDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? FinCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string Title { get; set; } = null!;
        public int DepartmentId { get; set; }
    }
}
