using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAssignmentRepository : IGenericRepository<Assignment>
    {
        Task<IEnumerable<Assignment>> GetByCourseOfferingAsync(int courseOfferingId);
        Task<Assignment?> GetWithSubmissionsAsync(int assignmentId);
    }
}
