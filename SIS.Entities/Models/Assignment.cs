using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    public class Assignment : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public AssignmentType Type { get; set; } = AssignmentType.Assignment;
        public DateTime DueDate { get; set; }
        public decimal MaxScore { get; set; }
        public bool IsVisible { get; set; } = true;
        public string? AttachmentUrl { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        /// <summary>
        /// Əgər bu tapşırıq rəsmi qiymətləndirmə planına bağlıdırsa, həmin AssessmentItem-ın ID-si.
        /// null = sadə ev tapşırığı (GPA-ya təsir etmir).
        /// </summary>
        public int? AssessmentItemId { get; set; }
        public AssessmentItem? AssessmentItem { get; set; }

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
