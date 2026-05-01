using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAcademicTermRepository : IGenericRepository<AcademicTerm>
    {
        Task<AcademicTerm?> GetActiveTermAsync();
        Task<IEnumerable<AcademicTerm>> GetAllTermsAsync();
    }
}
