using SIS.BLL.Services.DTOs.Department;

namespace SIS.BLL.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
        Task UpdateAsync(UpdateDepartmentDto dto);
        Task SetActiveAsync(int id, bool isActive);
        Task DeleteAsync(int id);
    }
}
