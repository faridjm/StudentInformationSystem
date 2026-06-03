using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ICourseMaterialRepository : IGenericRepository<CourseMaterial>
    {
        Task<IEnumerable<CourseMaterial>> GetByGroupAsync(int groupId);
        Task<IEnumerable<CourseMaterial>> GetVisibleByGroupAsync(int groupId);
    }
}
