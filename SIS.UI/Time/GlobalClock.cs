using SIS.Common.Time;

namespace SIS.UI.Time
{
    /// <summary>
    /// GlobalSimulationState-dən oxuyur. Bütün UI istifadəçiləri eyni tarixi görür.
    /// </summary>
    public class GlobalClock : IClock
    {
        private readonly GlobalSimulationState _state;

        public GlobalClock(GlobalSimulationState state)
        {
            _state = state;
        }

        public DateTime Now => _state.Date.HasValue
            ? _state.Date.Value.Date.Add(DateTime.Now.TimeOfDay)
            : DateTime.Now;

        public DateTime UtcNow => _state.Date.HasValue
            ? _state.Date.Value.Date.Add(DateTime.UtcNow.TimeOfDay)
            : DateTime.UtcNow;

        public DateOnly Today => DateOnly.FromDateTime(Now);
        public DateOnly TodayUtc => DateOnly.FromDateTime(UtcNow);
    }
}
