using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Assignment
{
    public class UpdateAssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public AssignmentType Type { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MaxScore { get; set; }
        public bool IsVisible { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
