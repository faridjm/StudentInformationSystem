using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Qrupun həftəlik cədvəli. Məs: Bazar ertəsi 18:00-20:00
    /// </summary>
    public class GroupScheduleDay : BaseEntity
    {
        public int GroupId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public Group Group { get; set; } = null!;
    }
}
