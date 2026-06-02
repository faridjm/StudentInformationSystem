namespace SIS.BLL.Services.DTOs.Chat
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string OtherUserName { get; set; } = null!;
        public string OtherUserId { get; set; } = null!;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string SenderUserId { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsMine { get; set; }
    }

    public class SendMessageDto
    {
        public int ConversationId { get; set; }
        public string Content { get; set; } = null!;
    }

    public class StartConversationDto
    {
        public int GroupId { get; set; }
        public string TargetUserId { get; set; } = null!;
    }
}
