using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(AppDbContext context) : base(context) { }

        public async Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId)
            => await _dbSet
                .Include(t => t.AppUser)
                .Include(t => t.Groups).ThenInclude(g => g.Course)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

        public async Task<Teacher?> GetTeacherByUserIdAsync(string userId)
            => await _dbSet
                .Include(t => t.AppUser)
                .FirstOrDefaultAsync(t => t.AppUserId == userId);

        public async Task<IEnumerable<Teacher>> GetAllWithUsersAsync()
            => await _dbSet.Include(t => t.AppUser).ToListAsync();
    }
}
