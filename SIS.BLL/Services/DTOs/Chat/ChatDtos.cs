using SIS.Entities.Enums;

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
        public string? Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsMine { get; set; }

        // Attachment
        public ChatAttachmentType AttachmentType { get; set; } = ChatAttachmentType.None;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }
        public long? AttachmentSize { get; set; }
        public int? DurationSeconds { get; set; }
    }

    public class SendMessageDto
    {
        public int ConversationId { get; set; }
        public string? Content { get; set; }
    }

    public class SendAttachmentMessageDto
    {
        public int ConversationId { get; set; }
        public string? Content { get; set; }
        public ChatAttachmentType AttachmentType { get; set; } = ChatAttachmentType.None;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }
        public long? AttachmentSize { get; set; }
        public int? DurationSeconds { get; set; }
    }

    public class StartConversationDto
    {
        public int GroupId { get; set; }
        public string TargetUserId { get; set; } = null!;
    }
}
