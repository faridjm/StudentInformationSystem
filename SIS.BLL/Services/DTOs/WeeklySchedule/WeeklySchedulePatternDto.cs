namespace SIS.BLL.Services.DTOs.WeeklySchedule
{
    public class WeeklySchedulePatternDto
    {
        public int Id { get; set; }
        public int CourseOfferingId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekName { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }
    }
}
