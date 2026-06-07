using Refit;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.SystemUsers;

namespace SIS.UI.ApiClients
{
    public interface ISystemUsersApi
    {
        [Get("/api/systemusers")]
        Task<PagedResult<SystemUserListItemDto>> GetPagedAsync(
            string? search, string? role, string? status, int page, int pageSize);

        [Get("/api/systemusers/{id}")]
        Task<SystemUserDetailDto> GetByIdAsync(string id);

        [Post("/api/systemusers")]
        Task<CreatedIdResponse> CreateAsync([Body] CreateSystemUserDto dto);

        [Put("/api/systemusers/{id}")]
        Task UpdateAsync(string id, [Body] UpdateSystemUserDto dto);

        [Post("/api/systemusers/{id}/change-password")]
        Task ChangePasswordAsync(string id, [Body] ChangeSystemUserPasswordDto dto);

        [Post("/api/systemusers/{id}/enable")]
        Task EnableAsync(string id);

        [Post("/api/systemusers/{id}/disable")]
        Task DisableAsync(string id);
    }

    public class CreatedIdResponse
    {
        public string Id { get; set; } = null!;
    }
}
