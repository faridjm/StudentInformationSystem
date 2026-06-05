using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Online classroom daxili canlı söhbət mesajı.
    /// Mövcud ChatMessage-dən (istifadəçi-istifadəçi söhbət) ayrılıb saxlanılır
    /// ki, classroom kontekstinə xas məntiqə təsir etməsin.
    /// </summary>
    public class OnlineLessonChatMessage : BaseEntity
    {
        public int OnlineLessonId { get; set; }

        /// <summary>Göndərənin AppUser.Id-si.</summary>
        public string SenderUserId { get; set; } = null!;

        /// <summary>Mesaj mətni (XSS qoruması render zamanı tətbiq olunur).</summary>
        public string MessageText { get; set; } = null!;

        public DateTime SentAt { get; set; }

        public OnlineLesson OnlineLesson { get; set; } = null!;
        public AppUser Sender { get; set; } = null!;
    }
}
