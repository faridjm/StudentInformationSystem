using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    public class AssignmentSubmission : BaseEntity
    {
        public string? Comment { get; set; }
        public string? FileUrl { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
        public bool IsGraded { get; set; } = false;

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;
    }
}
