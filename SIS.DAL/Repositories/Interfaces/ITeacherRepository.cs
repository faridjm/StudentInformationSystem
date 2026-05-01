using SIS.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ITeacherRepository : IGenericRepository<Teacher>
    {
        Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId);
        Task<Teacher?> GetTeacherByUserIdAsync(string userId);
        Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(int departmentId);
    }
}
