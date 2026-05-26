using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AcademicYearRepository : GenericRepository<AcademicYear>, IAcademicYearRepository
    {
        public AcademicYearRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<AcademicYear?> GetActiveYearAsync()
            => await _dbSet.FirstOrDefaultAsync(y => y.IsActive);

        public async Task<AcademicYear?> GetByNameAsync(string name)
            => await _dbSet.FirstOrDefaultAsync(y => y.Name == name);

        public async Task<IEnumerable<AcademicYear>> GetAllWithTermsAsync()
            => await _dbSet
                .Include(y => y.Terms)
                .OrderByDescending(y => y.StartDate)
                .ToListAsync();
    }
}
