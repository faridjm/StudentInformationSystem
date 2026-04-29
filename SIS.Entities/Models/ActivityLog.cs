using SIS.Entities.Base;
using SIS.Entities.Identity;

namespace SIS.Entities.Models
{
    public class ActivityLog : BaseEntity
    {
        public string Action { get; set; } = null!;      
        public string EntityName { get; set; } = null!;  
        public string? EntityId { get; set; }
        public string? OldValue { get; set; }             
        public string? NewValue { get; set; }         
        public string? IpAddress { get; set; }
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;
    }


}
