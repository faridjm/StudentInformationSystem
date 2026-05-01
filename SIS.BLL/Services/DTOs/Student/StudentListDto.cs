namespace SIS.BLL.Services.DTOs.Student
{
    public class StudentListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public string ProgramName { get; set; } = null!;
        public int EnrollmentYear { get; set; }
        public decimal CurrentGPA { get; set; }
        public bool IsActive { get; set; }
    }
}

