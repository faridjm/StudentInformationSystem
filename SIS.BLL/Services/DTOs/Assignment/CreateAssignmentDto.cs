using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Assignment
{
    public class CreateAssignmentDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public AssignmentType Type { get; set; } = AssignmentType.Assignment;
        public DateTime DueDate { get; set; }
        public decimal MaxScore { get; set; }
        public bool IsVisible { get; set; } = true;
        public string? AttachmentUrl { get; set; }
        public int GroupId { get; set; }
        /// <summary>Rəsmi assessment-ə bağlamaq üçün. null = sadə ev tapşırığı.</summary>
        public int? AssessmentItemId { get; set; }
    }
}
