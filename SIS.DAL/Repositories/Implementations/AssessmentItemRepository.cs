using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AssessmentItemRepository : GenericRepository<AssessmentItem>, IAssessmentItemRepository
    {
        public AssessmentItemRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<AssessmentItem>> GetByGroupAsync(int groupId)
            => await _dbSet
                .Where(a => a.GroupId == groupId)
                .OrderBy(a => a.Date)
                .ToListAsync();

        public async Task<AssessmentItem?> GetWithSubmissionsAsync(int itemId)
            => await _dbSet
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Enrollment)
                        .ThenInclude(e => e.Student)
                            .ThenInclude(st => st.AppUser)
                .FirstOrDefaultAsync(a => a.Id == itemId);
    }
}
