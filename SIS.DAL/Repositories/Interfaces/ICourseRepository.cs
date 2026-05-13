using SIS.Common.Pagination;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course?> GetCourseWithDetailsAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId);
        Task<bool> IsCourseCodeUniqueAsync(string code);
        Task<PagedResult<Course>> GetPagedAsync(PagedRequest request);
    }
}
