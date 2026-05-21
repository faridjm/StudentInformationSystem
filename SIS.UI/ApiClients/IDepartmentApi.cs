using Refit;
using SIS.BLL.Services.DTOs.Department;
using SIS.Common.Pagination;

namespace SIS.UI.ApiClients
{
    public interface IDepartmentApi
    {
        [Get("/api/department")]
        Task<PagedResult<DepartmentDto>> GetAllAsync([Query] PagedRequest request);

        [Get("/api/department/{id}")]
        Task<DepartmentDto> GetByIdAsync(int id);

        [Post("/api/department")]
        Task CreateAsync([Body] CreateDepartmentDto dto);

        [Put("/api/department")]
        Task UpdateAsync([Body] UpdateDepartmentDto dto);

        [Delete("/api/department/{id}")]
        Task DeleteAsync(int id);
    }


}
