using SIS.Common.Pagination;
using SIS.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<Student?> GetStudentWithDetailsAsync(int studentId);
        Task<Student?> GetStudentByUserIdAsync(string userId);
        Task<IEnumerable<Student>> GetStudentsByProgramAsync(int programId);
        Task<IEnumerable<Student>> GetStudentsWithGPAAsync();
        Task<PagedResult<Student>> GetPagedAsync(PagedRequest request);
    }
}
