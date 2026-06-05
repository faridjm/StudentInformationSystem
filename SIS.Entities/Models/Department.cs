using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Akademik şöbə / kafedra. Bir Department çoxlu Course / Programme saxlayır.
    /// Əlaqə: Department → Courses (one-to-many, optional).
    /// </summary>
    public class Department : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
