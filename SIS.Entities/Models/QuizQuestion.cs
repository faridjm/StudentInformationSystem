using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Quiz sualı. Hər sualın balı (Points) var; ümumi bal sualların cəmidir.
    /// </summary>
    public class QuizQuestion : BaseEntity
    {
        public int QuizId { get; set; }
        public string Text { get; set; } = null!;
        public QuestionType Type { get; set; }
        public decimal Points { get; set; } = 1;
        public int Order { get; set; }

        public Quiz Quiz { get; set; } = null!;
        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
    }
}
