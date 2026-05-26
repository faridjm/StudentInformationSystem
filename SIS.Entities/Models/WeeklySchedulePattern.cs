using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Müəllimin hər kurs üçün qoyduğu həftəlik şablon.
    /// Məs. "Hər Bazar ertəsi 09:00-10:30, otaq A101".
    /// Bu şablondan həqiqi ClassSession-lar generasiya olunur.
    /// </summary>
    public class WeeklySchedulePattern : BaseEntity
    {
        public int CourseOfferingId { get; set; }
        public CourseOffering CourseOffering { get; set; } = null!;

        /// <summary>Həftənin günü (Sunday=0, Monday=1, ..., Saturday=6).</summary>
        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        /// <summary>Standart otaq. Hər ClassSession ayrıca otaq seçə bilər.</summary>
        public string? Room { get; set; }
    }
}
