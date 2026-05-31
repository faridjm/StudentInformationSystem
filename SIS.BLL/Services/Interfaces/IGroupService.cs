using SIS.BLL.Services.DTOs.Groups;

namespace SIS.BLL.Services.Interfaces
{
    public interface IGroupService
    {
        Task<IEnumerable<GroupListDto>> GetAllAsync();
        Task<GroupDetailDto?> GetByIdAsync(int id);
        Task<IEnumerable<GroupListDto>> GetByTeacherAsync(int teacherId);
        Task<GroupDetailDto> CreateAsync(CreateGroupDto dto);
        Task UpdateAsync(UpdateGroupDto dto);
        Task DeleteAsync(int id);
        Task GenerateSessionsAsync(int groupId);
    }
}
