using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class Teacher : BaseEntity
    {
        public string EmployeeNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public string AppUserId { get; set; } = null!;
        public int DepartmentId { get; set; }
        public AppUser AppUser { get; set; } = null!;
        public Department Department { get; set; } = null!;
        public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    }
}
