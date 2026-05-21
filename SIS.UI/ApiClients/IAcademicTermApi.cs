using Refit;
using SIS.BLL.Services.DTOs.AcademicTerm;

namespace SIS.UI.ApiClients
{
    public interface IAcademicTermApi
    {
        [Get("/api/academicterm")]
        Task<IEnumerable<AcademicTermDto>> GetAllAsync();

        [Get("/api/academicterm/active")]
        Task<AcademicTermDto> GetActiveAsync();

        [Get("/api/academicterm/{id}")]
        Task<AcademicTermDto> GetByIdAsync(int id);

        [Post("/api/academicterm")]
        Task CreateAsync([Body] CreateAcademicTermDto dto);

        [Put("/api/academicterm")]
        Task UpdateAsync([Body] UpdateAcademicTermDto dto);

        [Put("/api/academicterm/{id}/set-active")]
        Task SetActiveAsync(int id);

        [Delete("/api/academicterm/{id}")]
        Task DeleteAsync(int id);
    }


}
