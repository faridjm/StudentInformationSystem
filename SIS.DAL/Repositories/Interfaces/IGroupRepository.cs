using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IGroupRepository : IGenericRepository<Group>
    {
        Task<Group?> GetGroupWithDetailsAsync(int groupId);
        Task<IEnumerable<Group>> GetByTeacherAsync(int teacherId);
        Task<IEnumerable<Group>> GetByCourseAsync(int courseId);
        Task<IEnumerable<Group>> GetActiveGroupsAsync();
    }
}
