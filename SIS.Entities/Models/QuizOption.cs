using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Quiz sualının variantı. IsCorrect yalnız müəllimə/auto-grading-ə görünür.
    /// </summary>
    public class QuizOption : BaseEntity
    {
        public int QuizQuestionId { get; set; }
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public int Order { get; set; }

        public QuizQuestion QuizQuestion { get; set; } = null!;
    }
}
