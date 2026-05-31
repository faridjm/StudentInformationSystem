namespace SIS.BLL.Services.DTOs.ClassSession
{
    public class UpdateTopicDto
    {
        public string? Topic { get; set; }
    }

    /// <summary>Müəllim mövzu/qeyd/saat/otaq dəyişdirir.</summary>
    public class UpdateClassSessionDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }
        public string? Topic { get; set; }
        public string? Notes { get; set; }
    }
}
