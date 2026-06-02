using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class ChatConversation : BaseEntity
    {
        public int GroupId { get; set; }
        public string TeacherUserId { get; set; } = null!;
        public string StudentUserId { get; set; } = null!;

        public Group Group { get; set; } = null!;
        public AppUser TeacherUser { get; set; } = null!;
        public AppUser StudentUser { get; set; } = null!;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
