using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ITeacherRepository : IGenericRepository<Teacher>
    {
        Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId);
        Task<Teacher?> GetTeacherByUserIdAsync(string userId);
        Task<IEnumerable<Teacher>> GetAllWithUsersAsync();
    }
}
