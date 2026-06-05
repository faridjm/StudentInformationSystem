using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class ChatConversation : BaseEntity
    {
        public int GroupId { get; set; }
        public string TeacherUserId { get; set; } = null!;
        public string StudentUserId { get; set; } = null!;

        // Arxiv statusu (SuperAdmin tərəfindən)
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedByUserId { get; set; }

        public Group Group { get; set; } = null!;
        public AppUser TeacherUser { get; set; } = null!;
        public AppUser StudentUser { get; set; } = null!;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
