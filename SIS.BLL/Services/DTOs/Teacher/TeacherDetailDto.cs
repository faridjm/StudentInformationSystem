namespace SIS.BLL.Services.DTOs.Teacher
{
    public class TeacherDetailDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? FinCode { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string EmployeeNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public string? ProfilePhoto { get; set; }
        public bool IsActive { get; set; }
    }
}
