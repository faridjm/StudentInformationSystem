using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Admin roluna malik köməkçiyə SuperAdmin tərəfindən verilən müvəqqəti
    /// Admin panel girişi. Aktiv və vaxtı keçməmiş grant olduqda giriş icazə verilir.
    /// </summary>
    public class AdminAccessGrant : BaseEntity
    {
        public string AdminUserId { get; set; } = null!;
        public string GrantedBySuperAdminId { get; set; } = null!;
        public DateTime GrantedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBySuperAdminId { get; set; }
        public string? Reason { get; set; }
    }
}
