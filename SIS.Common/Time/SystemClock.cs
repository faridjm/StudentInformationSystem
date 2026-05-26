namespace SIS.Common.Time
{
    
    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
        public DateTime Now => DateTime.Now;
        public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
    }
}
