using Microsoft.EntityFrameworkCore;
using SIS.Common.Pagination;
using SIS.DAL.Context;
using SIS.DAL.Extensions;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Course>> GetAllAsync()
            => await _dbSet
                .Include(c => c.Department)
                .Include(c => c.AcademicProgram)
                .ToListAsync();

        public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
            => await _dbSet
                .Include(c => c.Department)
                .Include(c => c.AcademicProgram)
                .Include(c => c.CourseOfferings)
                    .ThenInclude(co => co.Teacher)
                        .ThenInclude(t => t.AppUser)
                .FirstOrDefaultAsync(c => c.Id == courseId);

        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId)
            => await _dbSet
                .Where(c => c.DepartmentId == departmentId)
                .ToListAsync();

        public async Task<bool> IsCourseCodeUniqueAsync(string code)
            => !await _dbSet.AnyAsync(c => c.Code == code);

        public async Task<PagedResult<Course>> GetPagedAsync(PagedRequest request)
        {
            var query = _dbSet
                .Include(c => c.Department)
                .Include(c => c.AcademicProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.Where(c =>
                    c.Name.Contains(request.SearchTerm) ||
                    c.Code.Contains(request.SearchTerm));

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "code" => request.SortDescending
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code),
                "credits" => request.SortDescending
                    ? query.OrderByDescending(c => c.Credits)
                    : query.OrderBy(c => c.Credits),
                _ => query.OrderBy(c => c.Name)
            };

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }
    }


}

