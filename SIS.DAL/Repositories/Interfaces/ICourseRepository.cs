using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<IEnumerable<Course>> GetActiveCoursesAsync();
    }
}
