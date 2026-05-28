using SIS.Common.Time;

namespace SIS.API.Time
{
    /// <summary>
    /// API tərəfdə simulasiya rejimi. UI X-Simulation-Date header göndərirsə
    /// o tarixi "bu gün" kimi qəbul edir. Göndərməsə real vaxt istifadə olunur.
    /// </summary>
    public class HttpHeaderClock : IClock
    {
        private readonly IHttpContextAccessor _accessor;
        public const string HeaderName = "X-Simulation-Date";

        public HttpHeaderClock(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public DateTime UtcNow => GetSimDate()?.ToUniversalTime() ?? DateTime.UtcNow;
        public DateTime Now => GetSimDate() ?? DateTime.Now;
        public DateOnly Today => DateOnly.FromDateTime(Now);
        public DateOnly TodayUtc => DateOnly.FromDateTime(UtcNow);

        private DateTime? GetSimDate()
        {
            var ctx = _accessor.HttpContext;
            if (ctx == null) return null;

            if (ctx.Request.Headers.TryGetValue(HeaderName, out var val)
                && DateTime.TryParse(val.ToString(), out var dt))
            {
                return dt.Date.Add(DateTime.Now.TimeOfDay);
            }
            return null;
        }
    }
}
