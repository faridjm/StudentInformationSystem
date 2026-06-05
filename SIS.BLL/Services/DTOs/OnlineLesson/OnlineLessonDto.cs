using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.OnlineLesson
{
    /// <summary>Online dərs oxu (read) modeli.</summary>
    public class OnlineLessonDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ClassSessionId { get; set; }
        public int TeacherId { get; set; }
        public string GroupName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public int SessionNumber { get; set; }
        public DateTime SessionDate { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? MeetingUrl { get; set; }
        public OnlinePlatform? Platform { get; set; }
        public OnlineLessonStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int ParticipantCount { get; set; }
    }

    /// <summary>Online dərs yaratma modeli. MeetingUrl/Platform boş olarsa qrup default-ları istifadə olunur.</summary>
    public class CreateOnlineLessonDto
    {
        public int ClassSessionId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MeetingUrl { get; set; }
        public OnlinePlatform? Platform { get; set; }
    }

    /// <summary>Online dərs yeniləmə modeli (link/platform daxili classroom üçün lazım deyil; opsional saxlanılır).</summary>
    public class UpdateOnlineLessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? MeetingUrl { get; set; }
        public OnlinePlatform? Platform { get; set; }
    }
}

namespace SIS.BLL.Services.DTOs.OnlineLesson
{
    /// <summary>Online dərs iştirakçısı (qoşulan tələbə) modeli.</summary>
    public class OnlineLessonParticipantDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsAttendanceMarked { get; set; }
    }

    /// <summary>Tələbənin dərsə qoşulma cəhdinin nəticəsi.</summary>
    public class JoinOnlineLessonResultDto
    {
        public bool CanJoin { get; set; }
        public int? LessonId { get; set; }

        /// <summary>Daxili classroom səhifəsinin URL-i (məs. /Student/OnlineLessons/Classroom/{id}).</summary>
        public string? ClassroomUrl { get; set; }

        /// <summary>Köhnə external link uyğunluğu üçün saxlanılır (deprecated).</summary>
        public string? MeetingUrl { get; set; }

        public OnlineLessonStatus Status { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>Müəllim dərsi başladanda qaytarılan classroom yönləndirmə nəticəsi.</summary>
    public class StartOnlineLessonResultDto
    {
        public int LessonId { get; set; }
        public string ClassroomUrl { get; set; } = "";
    }

    /// <summary>Online classroom daxili canlı söhbət mesajı modeli.</summary>
    public class OnlineLessonChatMessageDto
    {
        public int Id { get; set; }
        public int OnlineLessonId { get; set; }
        public string SenderUserId { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string MessageText { get; set; } = "";
        public DateTime SentAt { get; set; }
    }

    /// <summary>Classroom hub-ından gələn mesaj göndərmə body-si.</summary>
    public class SendClassroomChatDto
    {
        public string Text { get; set; } = "";
    }

    /// <summary>"Qoşulanları Present işarələ" əməliyyatının nəticəsi.</summary>
    public class MarkPresentResultDto
    {
        public int MarkedCount { get; set; }
        public string Message { get; set; } = "";
    }
}
