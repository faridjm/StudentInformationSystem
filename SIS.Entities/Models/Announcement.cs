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

        /// <summary>Adminin "Email olaraq da göndər" seçimi.</summary>
        public bool SendEmail { get; set; }

        /// <summary>Toplu email göndəriş prosesinin tamamlandığı vaxt (ilk uğurlu push-dan sonra).</summary>
        public DateTime? EmailSentAt { get; set; }

        public AppUser Author { get; set; } = null!;
        public Group? Group { get; set; }

        /// <summary>Bu elanı alan istifadəçilər (per-user oxunma və email statusu).</summary>
        public ICollection<AnnouncementRecipient> Recipients { get; set; } = new List<AnnouncementRecipient>();
    }
}
