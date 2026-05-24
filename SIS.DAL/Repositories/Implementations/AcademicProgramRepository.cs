using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AcademicProgramRepository : GenericRepository<AcademicProgram>, IAcademicProgramRepository
    {
        public AcademicProgramRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<AcademicProgram?> GetProgramWithDetailsAsync(int programId)
            => await _dbSet
                .Include(p => p.Department)
                .Include(p => p.Students)
                    .ThenInclude(s => s.AppUser)
                .FirstOrDefaultAsync(p => p.Id == programId);

        public async Task<IEnumerable<AcademicProgram>> GetProgramsByDepartmentAsync(int departmentId)
            => await _dbSet
                .Where(p => p.DepartmentId == departmentId && p.IsActive)
                .ToListAsync();

        public async Task<IEnumerable<AcademicProgram>> GetAllWithDepartmentAsync()
            => await _dbSet
        .Include(p => p.Department)
        .ToListAsync();

        public override async Task<IEnumerable<AcademicProgram>> GetAllAsync()
            => await _dbSet
                .Include(p => p.Department)
                .Include(p => p.Students)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

    }
}

