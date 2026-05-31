using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAssignmentRepository : IGenericRepository<Assignment>
    {
        Task<IEnumerable<Assignment>> GetByGroupAsync(int groupId);
        Task<Assignment?> GetWithSubmissionsAsync(int assignmentId);
    }
}
