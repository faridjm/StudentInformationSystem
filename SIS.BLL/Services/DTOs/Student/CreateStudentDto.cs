namespace SIS.BLL.Services.DTOs.Student
{
    public class CreateStudentDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FinCode { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public int EnrollmentYear { get; set; }
    }
}
