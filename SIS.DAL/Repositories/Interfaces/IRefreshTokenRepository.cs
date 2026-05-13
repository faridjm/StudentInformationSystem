using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeAllForUserAsync(string userId);
    }
}
