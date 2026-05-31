using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Tədris sezonu. Məs: "2026-2027 Payız"
    /// </summary>
    public class AcademicYear : BaseEntity
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;

        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
