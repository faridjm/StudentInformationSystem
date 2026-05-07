using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
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
    }
}

