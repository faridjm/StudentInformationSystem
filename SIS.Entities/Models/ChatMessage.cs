using SIS.Entities.Base;
using SIS.Entities.Enums;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class ChatMessage : BaseEntity
    {
        public int ConversationId { get; set; }
        public string SenderUserId { get; set; } = null!;

        /// <summary>Mesaj mətni — attachment varsa boş ola bilər.</summary>
        public string? Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // Attachment dəstəyi (image / file / voice). Mesaj həm text həm attachment ola bilər.
        public ChatAttachmentType AttachmentType { get; set; } = ChatAttachmentType.None;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }
        public long? AttachmentSize { get; set; }

        /// <summary>Voice mesaj üçün saniyə ilə müddət.</summary>
        public int? DurationSeconds { get; set; }

        public ChatConversation Conversation { get; set; } = null!;
        public AppUser SenderUser { get; set; } = null!;
    }
}
