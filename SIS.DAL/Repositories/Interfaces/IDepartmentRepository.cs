using SIS.Common.Pagination;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<Department?> GetDepartmentWithDetailsAsync(int departmentId);
        Task<IEnumerable<Department>> GetActiveDepartmentsAsync();
        Task<PagedResult<Department>> GetPagedAsync(PagedRequest request);
    }
}
