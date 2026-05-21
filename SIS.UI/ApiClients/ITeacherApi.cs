using Refit;
using SIS.BLL.Services.DTOs.Teacher;
using SIS.Common.Pagination;

namespace SIS.UI.ApiClients
{
    public interface ITeacherApi
    {
        [Get("/api/teacher")]
        Task<PagedResult<TeacherListDto>> GetAllAsync([Query] PagedRequest request);

        [Get("/api/teacher/{id}")]
        Task<TeacherDetailDto> GetByIdAsync(int id);

        [Get("/api/teacher/me")]
        Task<TeacherDetailDto> GetMeAsync();

        [Get("/api/teacher/department/{departmentId}")]
        Task<IEnumerable<TeacherListDto>> GetByDepartmentAsync(int departmentId);

        [Post("/api/teacher")]
        Task CreateAsync([Body] CreateTeacherDto dto);

        [Put("/api/teacher")]
        Task UpdateAsync([Body] UpdateTeacherDto dto);

        [Delete("/api/teacher/{id}")]
        Task DeleteAsync(int id);
    }


}
