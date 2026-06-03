namespace SIS.BLL.Services.DTOs.AdminAccess
{
    public class AdminAccessUserDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        /// <summary>Active | Inactive | Expired</summary>
        public string Status { get; set; } = "Inactive";
        public DateTime? GrantedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? Reason { get; set; }
    }

    public class ActivateAdminAccessDto
    {
        public string AdminUserId { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
    }

    public class DeactivateAdminAccessDto
    {
        public string AdminUserId { get; set; } = null!;
        public string? Reason { get; set; }
    }
}
