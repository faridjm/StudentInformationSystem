using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Tələbənin qiymətləndirmə elementi üzrə cavabı/qiyməti.
    /// </summary>
    public class AssessmentSubmission : BaseEntity
    {
        public int AssessmentItemId { get; set; }
        public int EnrollmentId { get; set; }
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
        public bool IsGraded { get; set; } = false;
        public DateTime? GradedAt { get; set; }
        public string? FileUrl { get; set; }
        public string? Comment { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public AssessmentItem AssessmentItem { get; set; } = null!;
        public Enrollment Enrollment { get; set; } = null!;
    }
}
