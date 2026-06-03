using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Tələbənin quiz cəhdi. Hər tələbə (enrollment) bir quiz-ə yalnız bir dəfə cəhd edə bilər.
    /// Score = düz cavab faizi (0–100). Bu faiz AssessmentItem.MaxScore-a miqyaslanaraq
    /// AssessmentSubmission-a yazılır.
    /// </summary>
    public class QuizAttempt : BaseEntity
    {
        public int QuizId { get; set; }
        public int EnrollmentId { get; set; }
        public decimal? Score { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsSubmitted { get; set; } = false;

        public Quiz Quiz { get; set; } = null!;
        public Enrollment Enrollment { get; set; } = null!;
        public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}
