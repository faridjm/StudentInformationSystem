using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Announcement
{
    public class CreateAnnouncementDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public AnnouncementTarget TargetType { get; set; }
        public int? GroupId { get; set; }
        public bool IsGlobal { get; set; } = true;
        public string? AttachmentUrl { get; set; }
    }
}
