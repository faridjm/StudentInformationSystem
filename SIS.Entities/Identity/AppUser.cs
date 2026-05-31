using Microsoft.AspNetCore.Identity;
using SIS.Entities.Models;

namespace SIS.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? FinCode { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ProfilePhoto { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Student? Student { get; set; }
        public Teacher? Teacher { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}
