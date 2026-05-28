using SIS.Common.Time;

namespace SIS.UI.Time
{
    /// <summary>
    /// Simulasiya rejimi üçün IClock. Session-da SimulationDate varsa onu qaytarır,
    /// yoxdursa real vaxtı qaytarır. Hər istifadəçinin öz simulasiya tarixi olur.
    /// </summary>
    public class SessionClock : IClock
    {
        private readonly IHttpContextAccessor _accessor;
        public const string SessionKey = "SimulationDate";

        public SessionClock(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public DateTime UtcNow => GetSimDate()?.ToUniversalTime() ?? DateTime.UtcNow;
        public DateTime Now => GetSimDate() ?? DateTime.Now;
        public DateOnly Today => DateOnly.FromDateTime(Now);
        public DateOnly TodayUtc => DateOnly.FromDateTime(UtcNow);

        private DateTime? GetSimDate()
        {
            try
            {
                var ctx = _accessor.HttpContext;
                if (ctx?.Session == null) return null;
                var s = ctx.Session.GetString(SessionKey);
                if (string.IsNullOrEmpty(s)) return null;
                if (DateTime.TryParse(s, out var dt))
                    return dt.Date.Add(DateTime.Now.TimeOfDay); // tarixi simdan, saatı realdan
                return null;
            }
            catch { return null; }
        }

        /// <summary>Session-da simulasiya tarixi varmı.</summary>
        public static bool IsActive(HttpContext? ctx)
        {
            try
            {
                return !string.IsNullOrEmpty(ctx?.Session?.GetString(SessionKey));
            }
            catch { return false; }
        }

        /// <summary>Session-dakı simulasiya tarixini götür.</summary>
        public static string? GetDateString(HttpContext? ctx)
        {
            try { return ctx?.Session?.GetString(SessionKey); }
            catch { return null; }
        }
    }
}
