namespace SIS.BLL.Services.DTOs.Teacher
{
    public class TeacherDetailDto
    {
        public int Id { get; set; }
        public string AppUserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? FinCode { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string EmployeeNumber { get; set; } = null!;
        public string? Title { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
