using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AcademicTermRepository : GenericRepository<AcademicTerm>, IAcademicTermRepository
    {
        public AcademicTermRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<AcademicTerm?> GetActiveTermAsync()
            => await _dbSet
                .FirstOrDefaultAsync(t => t.IsActive);

        public async Task<IEnumerable<AcademicTerm>> GetAllTermsAsync()
            => await _dbSet
                .Include(t => t.AcademicYear)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
    }
}

