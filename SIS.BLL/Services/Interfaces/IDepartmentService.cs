using SIS.BLL.Services.DTOs.Department;

namespace SIS.BLL.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task CreateDepartmentAsync(CreateDepartmentDto dto);
        Task UpdateDepartmentAsync(UpdateDepartmentDto dto);
        Task DeleteDepartmentAsync(int id);
    }

}
