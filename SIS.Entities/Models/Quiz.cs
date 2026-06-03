using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Real suallı quiz. Mütləq Type=Quiz olan mövcud AssessmentItem-ə bağlıdır (1:1).
    /// Quiz ayrıca qiymətləndirmə yaratmır — nəticə bağlı AssessmentItem-in
    /// AssessmentSubmission-una yazılır və beləliklə Official Grade/GPA/Transcript-ə daxil olur.
    /// </summary>
    public class Quiz : BaseEntity
    {
        public int AssessmentItemId { get; set; }
        public int GroupId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = false;
        public int? DurationMinutes { get; set; }

        public AssessmentItem AssessmentItem { get; set; } = null!;
        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}
