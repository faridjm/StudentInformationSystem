using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAcademicYearRepository : IGenericRepository<AcademicYear>
    {
        Task<AcademicYear?> GetActiveYearAsync();
        Task<AcademicYear?> GetByNameAsync(string name);
        Task<IEnumerable<AcademicYear>> GetAllWithTermsAsync();
    }
}
