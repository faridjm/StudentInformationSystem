using SIS.BLL.Services.DTOs.Teacher;

namespace SIS.BLL.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherListDto>> GetAllTeachersAsync();
        Task<TeacherDetailDto?> GetTeacherByIdAsync(int id);
        Task<TeacherDetailDto?> GetTeacherByUserIdAsync(string userId);
        Task CreateTeacherAsync(CreateTeacherDto dto);
        Task UpdateTeacherAsync(UpdateTeacherDto dto);
        Task DeleteTeacherAsync(int id);
    }
}
