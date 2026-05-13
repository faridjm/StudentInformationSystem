using Microsoft.EntityFrameworkCore;
using SIS.Common.Pagination;
using SIS.DAL.Context;
using SIS.DAL.Extensions;
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

        public async Task<PagedResult<Teacher>> GetPagedAsync(PagedRequest request)
        {
            var query = _dbSet
                .Include(t => t.AppUser)
                .Include(t => t.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.Where(t =>
                    t.AppUser.FullName.Contains(request.SearchTerm) ||
                    t.EmployeeNumber.Contains(request.SearchTerm) ||
                    t.AppUser.Email!.Contains(request.SearchTerm));

            query = request.SortBy?.ToLower() switch
            {
                "fullname" => request.SortDescending
                    ? query.OrderByDescending(t => t.AppUser.FullName)
                    : query.OrderBy(t => t.AppUser.FullName),
                "department" => request.SortDescending
                    ? query.OrderByDescending(t => t.Department.Name)
                    : query.OrderBy(t => t.Department.Name),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }
    }
}

