using SIS.Entities.Base;
using SIS.Entities.Enums;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class Announcement : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public AnnouncementTarget TargetType { get; set; }
        public string AuthorId { get; set; } = null!;
        public string? AttachmentUrl { get; set; }
        public bool IsGlobal { get; set; } = true;
        public int? GroupId { get; set; }
        public AppUser Author { get; set; } = null!;
        public Group? Group { get; set; }
    }
}
