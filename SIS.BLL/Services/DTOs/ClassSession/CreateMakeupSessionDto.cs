namespace SIS.BLL.Services.DTOs.ClassSession
{
    /// <summary>Ləğv olunmuş sessiyanın əvəzedicisi.</summary>
    public class CreateMakeupSessionDto
    {
        /// <summary>Hansı ləğv olunmuş sessiyanın əvəzidir.</summary>
        public int OriginalSessionId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }
        public string? Topic { get; set; }
    }
}
