using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class ChatMessage : BaseEntity
    {
        public int ConversationId { get; set; }
        public string SenderUserId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public ChatConversation Conversation { get; set; } = null!;
        public AppUser SenderUser { get; set; } = null!;
    }
}
