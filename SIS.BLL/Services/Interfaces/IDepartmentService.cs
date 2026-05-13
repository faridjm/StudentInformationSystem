using SIS.BLL.Services.DTOs.Department;
using SIS.Common.Pagination;

namespace SIS.BLL.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<PagedResult<DepartmentDto>> GetAllDepartmentsAsync(PagedRequest request);
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task CreateDepartmentAsync(CreateDepartmentDto dto);
        Task UpdateDepartmentAsync(UpdateDepartmentDto dto);
        Task DeleteDepartmentAsync(int id);
    }

}
