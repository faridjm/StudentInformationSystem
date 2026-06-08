using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context) { }

        public async Task<Student?> GetStudentWithDetailsAsync(int studentId)
            => await _dbSet
                .Include(s => s.AppUser)
                .Include(s => s.Enrollments).ThenInclude(e => e.Group).ThenInclude(g => g.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

        public async Task<Student?> GetStudentByUserIdAsync(string userId)
            => await _dbSet
                .Include(s => s.AppUser)
                .FirstOrDefaultAsync(s => s.AppUserId == userId);

        public async Task<IEnumerable<Student>> GetAllWithUsersAsync()
            => await _dbSet.Include(s => s.AppUser).Where(s => !s.IsDeleted).ToListAsync();

        public async Task<IEnumerable<string?>> GetAllStudentNumbersAsync()
            => await _context.Set<Student>()
                .IgnoreQueryFilters() // silinmişlər daxil — nömrə reuse olmasın
                .Select(s => s.StudentNumber)
                .ToListAsync();
    }
}
