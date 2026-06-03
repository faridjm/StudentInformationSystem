using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AdminAccessGrantRepository : GenericRepository<AdminAccessGrant>, IAdminAccessGrantRepository
    {
        public AdminAccessGrantRepository(AppDbContext context) : base(context) { }

        public async Task<AdminAccessGrant?> GetActiveGrantAsync(string adminUserId)
            => await _dbSet
                .Where(g => g.AdminUserId == adminUserId && g.IsActive && g.RevokedAt == null)
                .OrderByDescending(g => g.GrantedAt)
                .FirstOrDefaultAsync();

        public async Task<IEnumerable<AdminAccessGrant>> GetActiveGrantsAsync(string adminUserId)
            => await _dbSet
                .Where(g => g.AdminUserId == adminUserId && g.IsActive && g.RevokedAt == null)
                .ToListAsync();

        public async Task<AdminAccessGrant?> GetLatestGrantAsync(string adminUserId)
            => await _dbSet
                .Where(g => g.AdminUserId == adminUserId)
                .OrderByDescending(g => g.GrantedAt)
                .FirstOrDefaultAsync();
    }
}
