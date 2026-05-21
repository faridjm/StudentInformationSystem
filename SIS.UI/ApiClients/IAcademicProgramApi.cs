using Refit;
using SIS.BLL.Services.DTOs.AcademicProgram;

namespace SIS.UI.ApiClients
{
    public interface IAcademicProgramApi
    {
        [Get("/api/academicprogram")]
        Task<IEnumerable<AcademicProgramDto>> GetAllAsync();

        [Get("/api/academicprogram/{id}")]
        Task<AcademicProgramDto> GetByIdAsync(int id);

        [Get("/api/academicprogram/department/{departmentId}")]
        Task<IEnumerable<AcademicProgramDto>> GetByDepartmentAsync(int departmentId);

        [Post("/api/academicprogram")]
        Task CreateAsync([Body] CreateAcademicProgramDto dto);

        [Put("/api/academicprogram")]
        Task UpdateAsync([Body] UpdateAcademicProgramDto dto);

        [Delete("/api/academicprogram/{id}")]
        Task DeleteAsync(int id);
    }


}
