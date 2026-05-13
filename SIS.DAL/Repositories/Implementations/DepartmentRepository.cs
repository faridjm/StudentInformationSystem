using Microsoft.EntityFrameworkCore;
using SIS.Common.Pagination;
using SIS.DAL.Context;
using SIS.DAL.Extensions;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Department?> GetDepartmentWithDetailsAsync(int departmentId)
            => await _dbSet
                .Include(d => d.Programs)
                .Include(d => d.Teachers)
                    .ThenInclude(t => t.AppUser)
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
            => await _dbSet
                .Where(d => d.IsActive)
                .ToListAsync();

        public async Task<PagedResult<Department>> GetPagedAsync(PagedRequest request)
        {
            var query = _dbSet
                .Include(d => d.Programs)
                .Include(d => d.Teachers)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.Where(d =>
                    d.Name.Contains(request.SearchTerm) ||
                    d.Code.Contains(request.SearchTerm));

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending
                    ? query.OrderByDescending(d => d.Name)
                    : query.OrderBy(d => d.Name),
                "code" => request.SortDescending
                    ? query.OrderByDescending(d => d.Code)
                    : query.OrderBy(d => d.Code),
                _ => query.OrderBy(d => d.Name)
            };

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }
    }
}

