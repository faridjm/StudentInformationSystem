using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class Student : BaseEntity
    {
        public string StudentNumber { get; set; } = null!;
        public int EnrollmentYear { get; set; }
        public decimal CurrentGPA { get; set; } = 0;
        public string AppUserId { get; set; } = null!;
        public int AcademicProgramId { get; set; }
        public AppUser AppUser { get; set; } = null!;
        public AcademicProgram AcademicProgram { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
