using Refit;
using SIS.BLL.Services.DTOs.Teacher;

namespace SIS.UI.ApiClients
{
    public interface ITeacherApi
    {
        [Get("/api/teacher")]
        Task<IEnumerable<TeacherListDto>> GetAllAsync();

        [Get("/api/teacher/{id}")]
        Task<TeacherDetailDto> GetByIdAsync(int id);

        [Get("/api/teacher/me")]
        Task<TeacherDetailDto> GetMeAsync();

        [Post("/api/teacher")]
        Task CreateAsync([Body] CreateTeacherDto dto);

        [Put("/api/teacher")]
        Task UpdateAsync([Body] UpdateTeacherDto dto);

        [Delete("/api/teacher/{id}")]
        Task DeleteAsync(int id);
    }
}
