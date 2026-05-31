using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Tədris kursu/istiqaməti. Məs: "Proqramlaşdırma", "Data Analytics", "AI/ML"
    /// </summary>
    public class Course : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
