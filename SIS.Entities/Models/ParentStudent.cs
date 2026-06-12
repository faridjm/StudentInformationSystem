using SIS.Entities.Base;
using SIS.Entities.Enums;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Valideyn (AppUser, Parent rolu) ilə tələbə arasında əlaqə.
    /// Bir valideyn bir neçə tələbəyə, bir tələbə bir neçə valideynə bağlana bilər.
    /// Parent yalnız IsActive = true əlaqə ilə bağlı tələbəni görə bilər (read-only).
    /// </summary>
    public class ParentStudent : BaseEntity
    {
        public string ParentUserId { get; set; } = null!;
        public AppUser ParentUser { get; set; } = null!;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public RelationshipType RelationshipType { get; set; } = RelationshipType.Guardian;
        public bool IsActive { get; set; } = true;
    }
}
