using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Assignment
{
    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public AssignmentType Type { get; set; }
        public string TypeLabel => Type switch
        {
            AssignmentType.Assignment => "Tapşırıq",
            AssignmentType.Quiz => "Quiz",
            AssignmentType.Project => "Layihə",
            AssignmentType.Exam => "İmtahan",
            _ => Type.ToString()
        };
        public DateTime DueDate { get; set; }
        public decimal MaxScore { get; set; }
        public bool IsVisible { get; set; }
        public string? AttachmentUrl { get; set; }
        public int CourseOfferingId { get; set; }
        public int SubmissionCount { get; set; }
    }
}
