using SIS.BLL.Services.DTOs.AcademicProgram;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAcademicProgramService
    {
        Task<IEnumerable<AcademicProgramDto>> GetAllProgramsAsync();
        Task<AcademicProgramDto?> GetProgramByIdAsync(int id);
        Task<IEnumerable<AcademicProgramDto>> GetProgramsByDepartmentAsync(int departmentId);
        Task CreateProgramAsync(CreateAcademicProgramDto dto);
        Task UpdateProgramAsync(UpdateAcademicProgramDto dto);
        Task DeleteProgramAsync(int id);
    }
}
