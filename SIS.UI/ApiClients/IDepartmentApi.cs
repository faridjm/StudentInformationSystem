using Refit;
using SIS.BLL.Services.DTOs.Department;

namespace SIS.UI.ApiClients
{
    public interface IDepartmentApi
    {
        [Get("/api/department")]
        Task<IEnumerable<DepartmentDto>> GetAllAsync();

        [Get("/api/department/{id}")]
        Task<DepartmentDto> GetByIdAsync(int id);

        [Post("/api/department")]
        Task<DepartmentDto> CreateAsync([Body] CreateDepartmentDto dto);

        [Put("/api/department")]
        Task UpdateAsync([Body] UpdateDepartmentDto dto);

        [Post("/api/department/{id}/active")]
        Task SetActiveAsync(int id, [Query] bool isActive);

        [Delete("/api/department/{id}")]
        Task DeleteAsync(int id);
    }
}
