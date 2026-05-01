using SIS.BLL.Services.DTOs.AcademicTerm;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAcademicTermService
    {
        Task<IEnumerable<AcademicTermDto>> GetAllTermsAsync();
        Task<AcademicTermDto?> GetActiveTermAsync();
        Task<AcademicTermDto?> GetTermByIdAsync(int id);
        Task CreateTermAsync(CreateAcademicTermDto dto);
        Task UpdateTermAsync(UpdateAcademicTermDto dto);
        Task SetActiveTermAsync(int id);
        Task DeleteTermAsync(int id);
    }
}
