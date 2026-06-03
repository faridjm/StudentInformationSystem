using SIS.BLL.Services.DTOs.AdminAccess;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAdminAccessService
    {
        Task<bool> HasActiveAdminAccessAsync(string adminUserId);
        Task ActivateAsync(string adminUserId, DateTime? expiresAt, string? reason, string grantedBySuperAdminId);
        Task DeactivateAsync(string adminUserId, string? reason, string revokedBySuperAdminId);
        Task<IEnumerable<AdminAccessUserDto>> GetAdminAccessUsersAsync();
    }
}
