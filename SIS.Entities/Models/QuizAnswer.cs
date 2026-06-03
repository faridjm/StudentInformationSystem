using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Bir suala verilmiş cavab. Seçilmiş variant id-ləri vergüllə ayrılmış saxlanılır
    /// (SingleChoice/TrueFalse üçün bir id, MultipleChoice üçün bir neçə).
    /// IsCorrect və PointsAwarded auto-grading zamanı hesablanır.
    /// </summary>
    public class QuizAnswer : BaseEntity
    {
        public int QuizAttemptId { get; set; }
        public int QuizQuestionId { get; set; }
        public string? SelectedOptionIds { get; set; }
        public bool IsCorrect { get; set; }
        public decimal PointsAwarded { get; set; }

        public QuizAttempt QuizAttempt { get; set; } = null!;
        public QuizQuestion QuizQuestion { get; set; } = null!;
    }
}
