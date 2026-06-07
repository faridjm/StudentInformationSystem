using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CourseApplicationRepository
        : GenericRepository<CourseApplication>, ICourseApplicationRepository
    {
        public CourseApplicationRepository(AppDbContext context) : base(context) { }

        public Task<CourseApplication?> GetWithRelationsAsync(int id)
            => _dbSet
                .Include(a => a.Course)
                .Include(a => a.Group)
                .Include(a => a.ConvertedStudent).ThenInclude(s => s!.AppUser)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<(IReadOnlyList<CourseApplication> Items, int TotalCount)> QueryAsync(
            string? search, CourseApplicationStatus? status, int? courseId,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            IQueryable<CourseApplication> q = _dbSet
                .Include(a => a.Course)
                .Include(a => a.Group)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(a =>
                    EF.Functions.Like(a.FullName, $"%{s}%") ||
                    EF.Functions.Like(a.Email, $"%{s}%") ||
                    EF.Functions.Like(a.Phone, $"%{s}%"));
            }
            if (status.HasValue)  q = q.Where(a => a.Status == status.Value);
            if (courseId.HasValue) q = q.Where(a => a.CourseId == courseId.Value);
            if (fromDate.HasValue) q = q.Where(a => a.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)   q = q.Where(a => a.CreatedAt <= toDate.Value);

            var total = await q.CountAsync();
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var items = await q
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
