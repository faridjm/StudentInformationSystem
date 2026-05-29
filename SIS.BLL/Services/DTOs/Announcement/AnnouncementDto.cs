using SIS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

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
    }


}
