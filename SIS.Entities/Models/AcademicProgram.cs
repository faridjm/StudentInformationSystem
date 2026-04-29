using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    public class AcademicProgram : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int DurationYears { get; set; }
        public bool IsActive { get; set; } = true;
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
