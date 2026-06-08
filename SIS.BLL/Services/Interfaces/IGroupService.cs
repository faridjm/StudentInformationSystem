using SIS.BLL.Services.DTOs.Groups;
using SIS.BLL.Services.DTOs.Performance;

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
        /// <summary>Qrupdakı bütün aktiv tələbələrin performans (davamiyyət+qiymət+GPA) cədvəli.</summary>
        Task<IEnumerable<GroupStudentPerformanceDto>> GetGroupPerformanceAsync(int groupId);
    }
}
