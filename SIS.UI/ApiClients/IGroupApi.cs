using Refit;
using SIS.BLL.Services.DTOs.Groups;

namespace SIS.UI.ApiClients
{
    public interface IGroupApi
    {
        [Get("/api/group")]
        Task<IEnumerable<GroupListDto>> GetAllAsync();

        [Get("/api/group/{id}")]
        Task<GroupDetailDto> GetByIdAsync(int id);

        [Get("/api/group/my-groups")]
        Task<IEnumerable<GroupListDto>> GetMyGroupsAsync();

        [Post("/api/group")]
        Task<GroupDetailDto> CreateAsync([Body] CreateGroupDto dto);

        [Put("/api/group")]
        Task UpdateAsync([Body] UpdateGroupDto dto);

        [Delete("/api/group/{id}")]
        Task DeleteAsync(int id);

        [Post("/api/group/{id}/generate-sessions")]
        Task GenerateSessionsAsync(int id);
    }
}
