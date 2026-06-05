using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Announcement
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public AnnouncementTarget TargetType { get; set; }
        public string? CourseName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsGlobal { get; set; }
        public string? AttachmentUrl { get; set; }

        // Yeni:
        public bool SendEmail { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public int RecipientCount { get; set; }
        public int ReadCount { get; set; }
        public int EmailSentCount { get; set; }
        public bool IsRead { get; set; }
    }

    public class AnnouncementRecipientRowDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Role { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool EmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
    }

    public class AnnouncementDetailDto : AnnouncementDto
    {
        public List<AnnouncementRecipientRowDto> Recipients { get; set; } = new();
    }

    public class UserSearchResultDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
