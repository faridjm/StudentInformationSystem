using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId)
            => await _dbSet
                .Include(t => t.AppUser)
                .Include(t => t.Department)
                .Include(t => t.CourseOfferings)
                    .ThenInclude(co => co.Course)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

        public async Task<Teacher?> GetTeacherByUserIdAsync(string userId)
            => await _dbSet
                .Include(t => t.AppUser)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.AppUserId == userId);

        public async Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(int departmentId)
            => await _dbSet
                .Include(t => t.AppUser)
                .Where(t => t.DepartmentId == departmentId)
                .ToListAsync();



        public async Task<IEnumerable<Teacher>> GetAllTeachersWithDetailsAsync()
             => await _dbSet
        .Include(t => t.AppUser)
        .Include(t => t.Department)
        .ToListAsync();
    }
}

