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

        public int CourseOfferingId { get; set; }
        public CourseOffering CourseOffering { get; set; } = null!;

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
