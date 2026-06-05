using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Elanı alan konkret istifadəçi: panel görüntüləməsi, oxunma və email statusu.
    /// </summary>
    public class AnnouncementRecipient : BaseEntity
    {
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public bool EmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
    }
}
