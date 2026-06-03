using SIS.BLL.Services.DTOs.Material;

namespace SIS.BLL.Services.Interfaces
{
    public interface ICourseMaterialService
    {
        Task<IEnumerable<CourseMaterialDto>> GetByGroupAsync(int groupId);
        Task<IEnumerable<CourseMaterialDto>> GetVisibleByGroupAsync(int groupId);
        Task<CourseMaterialDto?> GetByIdAsync(int id);
        Task<CourseMaterialDto> CreateAsync(CreateCourseMaterialDto dto, int uploadedByTeacherId);
        Task UpdateAsync(UpdateCourseMaterialDto dto);
        Task SetActiveAsync(int id, bool active);
        Task DeleteAsync(int id);
    }
}
