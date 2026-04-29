using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    public class Course : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    }
}
