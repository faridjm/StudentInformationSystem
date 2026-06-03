using Refit;
using SIS.BLL.Services.DTOs.AdminAccess;

namespace SIS.UI.ApiClients
{
    public interface IAdminAccessApi
    {
        [Get("/api/adminaccess/has-access")]
        Task<bool> HasAccessAsync();

        [Get("/api/adminaccess/users")]
        Task<IEnumerable<AdminAccessUserDto>> GetUsersAsync();

        [Post("/api/adminaccess/activate")]
        Task ActivateAsync([Body] ActivateAdminAccessDto dto);

        [Post("/api/adminaccess/deactivate")]
        Task DeactivateAsync([Body] DeactivateAdminAccessDto dto);
    }
}
