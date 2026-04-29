using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    public class Grade : BaseEntity
    {
        public GradeType Type { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Note { get; set; }
        public DateTime GradedAt { get; set; } = DateTime.UtcNow;
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;
    }
}
