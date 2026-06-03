using Refit;
using SIS.BLL.Services.DTOs.Material;

namespace SIS.UI.ApiClients
{
    public interface ICourseMaterialApi
    {
        [Get("/api/coursematerial/group/{groupId}")]
        Task<IEnumerable<CourseMaterialDto>> GetByGroupAsync(int groupId);

        [Get("/api/coursematerial/group/{groupId}/visible")]
        Task<IEnumerable<CourseMaterialDto>> GetVisibleByGroupAsync(int groupId);

        [Get("/api/coursematerial/{id}")]
        Task<CourseMaterialDto> GetByIdAsync(int id);

        [Post("/api/coursematerial")]
        Task<CourseMaterialDto> CreateAsync([Body] CreateCourseMaterialDto dto);

        [Put("/api/coursematerial")]
        Task UpdateAsync([Body] UpdateCourseMaterialDto dto);

        [Post("/api/coursematerial/{id}/active")]
        Task SetActiveAsync(int id, bool active);

        [Delete("/api/coursematerial/{id}")]
        Task DeleteAsync(int id);
    }
}
