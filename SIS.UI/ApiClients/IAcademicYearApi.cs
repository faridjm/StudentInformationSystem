using Refit;
using SIS.BLL.Services.DTOs.AcademicYear;

namespace SIS.UI.ApiClients
{
    public interface IAcademicYearApi
    {
        [Get("/api/academicyear")]
        Task<IEnumerable<AcademicYearDto>> GetAllAsync();

        [Get("/api/academicyear/active")]
        Task<AcademicYearDto> GetActiveAsync();

        [Get("/api/academicyear/{id}")]
        Task<AcademicYearDto> GetByIdAsync(int id);

        [Post("/api/academicyear")]
        Task CreateAsync([Body] CreateAcademicYearDto dto);

        [Put("/api/academicyear")]
        Task UpdateAsync([Body] UpdateAcademicYearDto dto);

        [Put("/api/academicyear/{id}/set-active")]
        Task SetActiveAsync(int id);

        [Delete("/api/academicyear/{id}")]
        Task DeleteAsync(int id);
    }
}
