using SIS.API.Services;
using SIS.Common.Time;

namespace SIS.API.Time
{
    /// <summary>
    /// SimulationService-dən oxuyur. Admin simulasiya aktiv etmişsə o tarixi,
    /// yoxdursa real tarixi qaytarır.
    /// </summary>
    public class GlobalClock : IClock
    {
        private readonly SimulationService _sim;

        public GlobalClock(SimulationService sim)
        {
            _sim = sim;
        }

        public DateTime Now => _sim.OverrideDate.HasValue
            ? _sim.OverrideDate.Value.Date.Add(DateTime.Now.TimeOfDay)
            : DateTime.Now;

        public DateTime UtcNow => _sim.OverrideDate.HasValue
            ? _sim.OverrideDate.Value.Date.Add(DateTime.UtcNow.TimeOfDay)
            : DateTime.UtcNow;

        public DateOnly Today => DateOnly.FromDateTime(Now);
        public DateOnly TodayUtc => DateOnly.FromDateTime(UtcNow);
    }
}
