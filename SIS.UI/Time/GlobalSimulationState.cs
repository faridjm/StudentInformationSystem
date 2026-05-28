namespace SIS.UI.Time
{
    /// <summary>
    /// UI tərəfindəki global simulasiya vəziyyəti. Singleton.
    /// API-dəki SimulationService ilə sinxronizasiya SimulationController vasitəsilə olur.
    /// </summary>
    public class GlobalSimulationState
    {
        private DateTime? _date;

        public DateTime? Date => _date;
        public bool IsActive => _date.HasValue;
        public string? DateString => _date?.ToString("yyyy-MM-dd");
        public string? DateDisplay => _date?.ToString("dd.MM.yyyy");

        public void Set(DateTime date) => _date = date.Date;
        public void Clear() => _date = null;
    }
}
