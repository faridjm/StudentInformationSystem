using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Enrollment
{
    public class EnrollmentDetailDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
        public string TermName { get; set; } = null!;
        public EnrollmentStatus Status { get; set; }
        public decimal AverageScore { get; set; }
        public double AttendancePercentage { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
