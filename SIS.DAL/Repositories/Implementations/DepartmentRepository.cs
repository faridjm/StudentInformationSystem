using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Department>> GetAllWithCoursesAsync()
            => await _dbSet.Include(d => d.Courses).OrderBy(d => d.Name).ToListAsync();

        public async Task<Department?> GetByIdWithCoursesAsync(int id)
            => await _dbSet.Include(d => d.Courses).FirstOrDefaultAsync(d => d.Id == id);
    }
}
