namespace SIS.BLL.Services.DTOs.ClassSession
{
    /// <summary>Müəllim əl ilə ayrıca sessiya yaratdıqda (məs. xüsusi dərs).</summary>
    public class CreateClassSessionDto
    {
        public int CourseOfferingId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }
        public string? Topic { get; set; }
        public string? Notes { get; set; }
    }
}
