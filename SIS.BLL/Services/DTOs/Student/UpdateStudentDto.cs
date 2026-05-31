namespace SIS.BLL.Services.DTOs.Student
{
    public class UpdateStudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? FinCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
