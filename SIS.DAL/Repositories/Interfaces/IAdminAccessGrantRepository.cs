using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAdminAccessGrantRepository : IGenericRepository<AdminAccessGrant>
    {
        Task<AdminAccessGrant?> GetActiveGrantAsync(string adminUserId);
        Task<IEnumerable<AdminAccessGrant>> GetActiveGrantsAsync(string adminUserId);
        Task<AdminAccessGrant?> GetLatestGrantAsync(string adminUserId);
    }
}
