using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAcademicProgramRepository : IGenericRepository<AcademicProgram>
    {
        Task<AcademicProgram?> GetProgramWithDetailsAsync(int programId);
        Task<IEnumerable<AcademicProgram>> GetProgramsByDepartmentAsync(int departmentId);
        Task<IEnumerable<AcademicProgram>> GetAllWithDepartmentAsync();
    }
}
