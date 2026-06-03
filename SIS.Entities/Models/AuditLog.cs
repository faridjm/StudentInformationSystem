using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Sistemdə baş verən əsas əməliyyatların audit qeydi.
    /// Həssas məlumatlar (parol, token və s.) burada saxlanılmır.
    /// </summary>
    public class AuditLog : BaseEntity
    {
        public string? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserRole { get; set; }
        public string Action { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public string? EntityId { get; set; }
        public string? Description { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
