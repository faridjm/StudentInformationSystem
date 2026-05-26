namespace SIS.Common.Time
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateOnly TodayUtc { get; }
        DateTime Now { get; }
        DateOnly Today { get; }
    }
}
