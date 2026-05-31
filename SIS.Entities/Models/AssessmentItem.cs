using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Qiymətləndirmə planı elementi. Quiz, Tapşırıq, Final Layihəsi.
    /// </summary>
    public class AssessmentItem : BaseEntity
    {
        public int GroupId { get; set; }
        public AssessmentType Type { get; set; }
        public string Title { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal MaxScore { get; set; }
        public decimal WeightPercent { get; set; }
        public string? Description { get; set; }
        public bool IsVisible { get; set; } = true;

        public Group Group { get; set; } = null!;
        public ICollection<AssessmentSubmission> Submissions { get; set; } = new List<AssessmentSubmission>();
    }
}
