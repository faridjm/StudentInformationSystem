using SIS.BLL.Services.DTOs.Teacher;
using SIS.Common.Pagination;

namespace SIS.BLL.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<PagedResult<TeacherListDto>> GetAllTeachersAsync(PagedRequest request);
        Task<TeacherDetailDto?> GetTeacherByIdAsync(int id);
        Task<TeacherDetailDto?> GetTeacherByUserIdAsync(string userId);
        Task<IEnumerable<TeacherListDto>> GetTeachersByDepartmentAsync(int departmentId);
        Task CreateTeacherAsync(CreateTeacherDto dto);
        Task UpdateTeacherAsync(UpdateTeacherDto dto);
        Task DeleteTeacherAsync(int id);
    }
}
