using Microsoft.EntityFrameworkCore;
using SIS.Common.Pagination;
using SIS.DAL.Context;
using SIS.DAL.Extensions;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.DAL.Repositories.Implementations
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Student?> GetStudentWithDetailsAsync(int studentId)
            => await _dbSet
                .Include(s => s.AppUser)
                .Include(s => s.AcademicProgram)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.CourseOffering)
                        .ThenInclude(co => co.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

        public async Task<Student?> GetStudentByUserIdAsync(string userId)
            => await _dbSet
                .Include(s => s.AppUser)
                .Include(s => s.AcademicProgram)
                .FirstOrDefaultAsync(s => s.AppUserId == userId);

        public async Task<IEnumerable<Student>> GetStudentsByProgramAsync(int programId)
            => await _dbSet
                .Include(s => s.AppUser)
                .Where(s => s.AcademicProgramId == programId)
                .ToListAsync();

        public async Task<IEnumerable<Student>> GetStudentsWithGPAAsync()
            => await _dbSet
                .Include(s => s.AppUser)
                .Include(s => s.AcademicProgram)
                .OrderByDescending(s => s.CurrentGPA)
                .ToListAsync();
        public async Task<PagedResult<Student>> GetPagedAsync(PagedRequest request)
        {
            var query = _dbSet
                .Include(s => s.AppUser)
                .Include(s => s.AcademicProgram)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.Where(s =>
                    s.AppUser.FullName.Contains(request.SearchTerm) ||
                    s.StudentNumber.Contains(request.SearchTerm) ||
                    s.AppUser.Email!.Contains(request.SearchTerm));

            // Sort
            query = request.SortBy?.ToLower() switch
            {
                "fullname" => request.SortDescending
                    ? query.OrderByDescending(s => s.AppUser.FullName)
                    : query.OrderBy(s => s.AppUser.FullName),
                "gpa" => request.SortDescending
                    ? query.OrderByDescending(s => s.CurrentGPA)
                    : query.OrderBy(s => s.CurrentGPA),
                "enrollmentyear" => request.SortDescending
                    ? query.OrderByDescending(s => s.EnrollmentYear)
                    : query.OrderBy(s => s.EnrollmentYear),
                _ => query.OrderByDescending(s => s.CreatedAt)
            };

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }
    }
}

