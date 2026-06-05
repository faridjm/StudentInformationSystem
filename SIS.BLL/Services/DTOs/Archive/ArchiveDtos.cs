using SIS.BLL.Services.DTOs.Common;

namespace SIS.BLL.Services.DTOs.Archive
{
    public class ArchiveChatFilterDto
    {
        public string? Search { get; set; }
        public string? Role { get; set; }       // "Student" | "Teacher" | null
        public string? Status { get; set; }     // "Active" | "Archived" | null
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? GroupId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ArchiveConversationListItemDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string CourseName { get; set; } = "";

        public string TeacherUserId { get; set; } = "";
        public string TeacherFullName { get; set; } = "";
        public string TeacherEmail { get; set; } = "";

        public string StudentUserId { get; set; } = "";
        public string StudentFullName { get; set; } = "";
        public string StudentEmail { get; set; } = "";

        public string LastMessagePreview { get; set; } = "";
        public DateTime? LastMessageAt { get; set; }
        public int MessageCount { get; set; }
        public int UnreadCount { get; set; }

        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedByName { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ArchiveConversationDetailDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string CourseName { get; set; } = "";

        public string TeacherUserId { get; set; } = "";
        public string TeacherFullName { get; set; } = "";
        public string TeacherEmail { get; set; } = "";

        public string StudentUserId { get; set; } = "";
        public string StudentFullName { get; set; } = "";
        public string StudentEmail { get; set; } = "";

        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedByName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int MessageCount { get; set; }

        public List<ArchiveMessageDto> Messages { get; set; } = new();
    }

    public class ArchiveMessageDto
    {
        public int Id { get; set; }
        public string SenderUserId { get; set; } = "";
        public string SenderFullName { get; set; } = "";
        public string SenderRole { get; set; } = "";   // "Teacher" | "Student"
        public string? Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        // Attachment (image/file/voice) — read-only display
        public Entities.Enums.ChatAttachmentType AttachmentType { get; set; } = Entities.Enums.ChatAttachmentType.None;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }
        public long? AttachmentSize { get; set; }
        public int? DurationSeconds { get; set; }
    }
}
