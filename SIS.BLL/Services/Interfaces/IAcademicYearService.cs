using SIS.BLL.Services.DTOs.AcademicYear;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAcademicYearService
    {
        Task<IEnumerable<AcademicYearDto>> GetAllYearsAsync();
        Task<AcademicYearDto?> GetYearByIdAsync(int id);
        Task<AcademicYearDto?> GetActiveYearAsync();
        Task CreateYearAsync(CreateAcademicYearDto dto);
        Task UpdateYearAsync(UpdateAcademicYearDto dto);
        Task SetActiveYearAsync(int id);
        Task DeleteYearAsync(int id);
    }
}
