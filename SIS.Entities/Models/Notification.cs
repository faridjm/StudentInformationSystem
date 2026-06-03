using SIS.Entities.Base;
using SIS.Entities.Enums;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public string? RelatedUrl { get; set; }
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;
    }


}
