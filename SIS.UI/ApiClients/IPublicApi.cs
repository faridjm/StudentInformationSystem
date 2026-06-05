using Refit;
using SIS.BLL.Services.DTOs.Public;

namespace SIS.UI.ApiClients
{
    public interface IPublicApi
    {
        [Get("/api/public/stats")]
        Task<PublicStatsDto> GetStatsAsync();

        [Get("/api/public/departments")]
        Task<IEnumerable<PublicDepartmentDto>> GetDepartmentsAsync();

        [Get("/api/public/courses")]
        Task<IEnumerable<PublicCourseDto>> GetCoursesAsync();

        [Get("/api/public/courses/{id}")]
        Task<PublicCourseDetailDto> GetCourseDetailAsync(int id);

        [Get("/api/public/teachers")]
        Task<IEnumerable<PublicTeacherDto>> GetTeachersAsync();
    }
}
