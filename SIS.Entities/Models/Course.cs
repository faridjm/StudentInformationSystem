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

        // F2 — Course / Programme kataloq məlumatları.
        // Mövcud sətirlər üçün təhlükəsiz: int-lər 0, Level null olur.
        public int Credit { get; set; }
        public int TotalHours { get; set; }
        public string? Level { get; set; }

        // F8 — Department əlaqəsi. Nullable: mövcud kurslar qırılmasın (Department boş qala bilər).
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
