using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<IEnumerable<Department>> GetAllWithCoursesAsync();
        Task<Department?> GetByIdWithCoursesAsync(int id);
    }
}
