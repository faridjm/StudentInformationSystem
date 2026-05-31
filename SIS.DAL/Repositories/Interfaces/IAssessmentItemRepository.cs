using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAssessmentItemRepository : IGenericRepository<AssessmentItem>
    {
        Task<IEnumerable<AssessmentItem>> GetByGroupAsync(int groupId);
        Task<AssessmentItem?> GetWithSubmissionsAsync(int itemId);
    }
}
