namespace SIS.BLL.Services.DTOs.Student
{
    public class StudentDetailDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string StudentNumber { get; set; } = null!;
        public string ProgramName { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public int EnrollmentYear { get; set; }
        public decimal CurrentGPA { get; set; }
        public string? ProfilePhoto { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}

