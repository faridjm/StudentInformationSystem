namespace SIS.BLL.Services.DTOs.WeeklySchedule
{
    /// <summary>Bir günlük slot — bulk yazılarkən istifadə olunur.</summary>
    public class WeeklySlotDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }
    }
}
