using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class Teacher : BaseEntity
    {
        public string EmployeeNumber { get; set; } = null!;
        public string? Title { get; set; }
        public DateTime HireDate { get; set; }
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
