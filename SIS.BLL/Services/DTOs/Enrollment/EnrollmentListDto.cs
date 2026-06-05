using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Enrollment
{
    public class EnrollmentListDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = null!;
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.InPerson;
    }
}
