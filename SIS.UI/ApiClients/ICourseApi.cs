using Refit;
using SIS.BLL.Services.DTOs.Course;

namespace SIS.UI.ApiClients
{
    public interface ICourseApi
    {
        [Get("/api/course")]
        Task<IEnumerable<CourseDto>> GetAllAsync();

        [Get("/api/course/{id}")]
        Task<CourseDto> GetByIdAsync(int id);

        [Post("/api/course")]
        Task<CourseDto> CreateAsync([Body] CreateCourseDto dto);

        [Put("/api/course")]
        Task UpdateAsync([Body] UpdateCourseDto dto);

        [Delete("/api/course/{id}")]
        Task DeleteAsync(int id);
    }
}
