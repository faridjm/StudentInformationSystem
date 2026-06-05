using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Tələbənin konkret online dərsə qoşulduğunu qeyd edən iştirak yazısı.
    /// (OnlineLessonId + StudentId) cütü üzrə unikaldır.
    /// Davamiyyəti avtomatik yaratmır — müəllim sonradan manual işarələyir.
    /// </summary>
    public class OnlineLessonParticipant : BaseEntity
    {
        public int OnlineLessonId { get; set; }
        public int StudentId { get; set; }

        /// <summary>Tələbənin qoşulduğu vaxt.</summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>Tələbənin ayrıldığı vaxt (opsional).</summary>
        public DateTime? LeftAt { get; set; }

        /// <summary>İştirak müddəti dəqiqə ilə (opsional).</summary>
        public int? DurationMinutes { get; set; }

        /// <summary>Bu iştirak əsasında davamiyyət işarələnibmi.</summary>
        public bool IsAttendanceMarked { get; set; }

        public OnlineLesson OnlineLesson { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
