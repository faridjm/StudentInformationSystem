using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    public class CourseOffering : BaseEntity
    {
        public int Capacity { get; set; }
        public string? Room { get; set; }
        public bool IsActive { get; set; } = true;
        public int AcademicTermId { get; set; }
        public int CourseId { get; set; }
        public int TeacherId { get; set; } 
        public AcademicTerm AcademicTerm { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public Teacher Teacher { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
