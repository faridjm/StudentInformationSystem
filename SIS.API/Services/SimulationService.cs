namespace SIS.API.Services
{
    /// <summary>
    /// Global simulasiya vəziyyəti. Singleton — bütün istifadəçilər eyni tarixi görür.
    /// Admin tərəfindən idarə olunur.
    /// </summary>
    public class SimulationService
    {
        private DateTime? _overrideDate;

        public DateTime? OverrideDate => _overrideDate;
        public bool IsActive => _overrideDate.HasValue;

        public void Set(DateTime date) => _overrideDate = date.Date;
        public void Clear() => _overrideDate = null;
    }
}
