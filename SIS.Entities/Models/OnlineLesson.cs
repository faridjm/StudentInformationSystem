using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Meeting link əsaslı online dərs (canlı dərs).
    /// Bir ClassSession-a bağlanır, qrupa və müəllimə aiddir.
    /// Video axını yoxdur — yalnız xarici görüş linki saxlanılır.
    /// </summary>
    public class OnlineLesson : BaseEntity
    {
        public int GroupId { get; set; }
        public int ClassSessionId { get; set; }
        public int TeacherId { get; set; }

        /// <summary>Dərsin başlığı (boş verilərsə qrup adından avtomatik qurulur).</summary>
        public string Title { get; set; } = null!;

        /// <summary>Əlavə təsvir (opsional).</summary>
        public string? Description { get; set; }

        /// <summary>Tələbənin qoşulduğu görüş linki.</summary>
        public string? MeetingUrl { get; set; }

        public OnlinePlatform? Platform { get; set; }
        public OnlineLessonStatus Status { get; set; } = OnlineLessonStatus.Scheduled;

        /// <summary>Dərs canlıya keçəndə doldurulur (opsional).</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>Dərs bitəndə doldurulur (opsional).</summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>Aktivlik bayrağı (soft-delete olan IsDeleted-dən ayrıdır).</summary>
        public bool IsActive { get; set; } = true;

        public Group Group { get; set; } = null!;
        public ClassSession ClassSession { get; set; } = null!;
        public Teacher Teacher { get; set; } = null!;

        public ICollection<OnlineLessonParticipant> Participants { get; set; } = new List<OnlineLessonParticipant>();
        public ICollection<OnlineLessonChatMessage> ChatMessages { get; set; } = new List<OnlineLessonChatMessage>();
    }
}
