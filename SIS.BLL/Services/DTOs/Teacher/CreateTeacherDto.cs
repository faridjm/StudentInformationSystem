namespace SIS.BLL.Services.DTOs.Teacher
{
    public class CreateTeacherDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FinCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string Title { get; set; } = null!;
        public int DepartmentId { get; set; }
        public DateTime HireDate { get; set; }
    }
}
