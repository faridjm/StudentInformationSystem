using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Announcement
{
    public class CreateAnnouncementDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public AnnouncementTarget TargetType { get; set; }

        /// <summary>TargetType = GroupStudents olduqda tələb olunur.</summary>
        public int? GroupId { get; set; }

        /// <summary>TargetType = SelectedUsers olduqda tələb olunur. AppUser.Id-lər.</summary>
        public List<string>? UserIds { get; set; }

        /// <summary>İşarələnibsə, recipient-lərə paneldən əlavə email də göndərilir.</summary>
        public bool SendEmail { get; set; }

        public bool IsGlobal { get; set; } = true;
        public string? AttachmentUrl { get; set; }
    }
}

namespace SIS.BLL.Services.DTOs.Announcement
{
    /// <summary>Müəllim öz qrupuna sadə elan göndərmək üçün model.</summary>
    public class TeacherGroupAnnouncementDto
    {
        public int GroupId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
