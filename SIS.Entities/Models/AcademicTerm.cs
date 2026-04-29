using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    public class AcademicTerm : BaseEntity
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;
        public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    }

}
