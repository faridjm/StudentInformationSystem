using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class ParentStudentRepository : GenericRepository<ParentStudent>, IParentStudentRepository
    {
        public ParentStudentRepository(AppDbContext context) : base(context) { }

        public async Task<List<ParentStudent>> GetByParentAsync(string parentUserId, bool onlyActive = false)
        {
            var q = _dbSet
                .Include(ps => ps.Student).ThenInclude(s => s.AppUser)
                .Where(ps => ps.ParentUserId == parentUserId);
            if (onlyActive) q = q.Where(ps => ps.IsActive);
            return await q.OrderBy(ps => ps.Id).ToListAsync();
        }

        public async Task<List<ParentStudent>> GetByStudentAsync(int studentId)
            => await _dbSet
                .Include(ps => ps.ParentUser)
                .Where(ps => ps.StudentId == studentId)
                .OrderBy(ps => ps.Id)
                .ToListAsync();

        public Task<ParentStudent?> GetRelationAsync(string parentUserId, int studentId)
            => _dbSet
                .Include(ps => ps.ParentUser)
                .FirstOrDefaultAsync(ps => ps.ParentUserId == parentUserId && ps.StudentId == studentId);

        public Task<bool> HasActiveAccessAsync(string parentUserId, int studentId)
            => _dbSet.AnyAsync(ps => ps.ParentUserId == parentUserId
                                     && ps.StudentId == studentId
                                     && ps.IsActive);
    }
}
