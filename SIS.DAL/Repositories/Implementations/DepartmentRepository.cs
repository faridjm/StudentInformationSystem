using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Department?> GetDepartmentWithDetailsAsync(int departmentId)
            => await _dbSet
                .Include(d => d.Programs)
                .Include(d => d.Teachers)
                    .ThenInclude(t => t.AppUser)
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
            => await _dbSet
                .Where(d => d.IsActive)
                .ToListAsync();
    }
}

