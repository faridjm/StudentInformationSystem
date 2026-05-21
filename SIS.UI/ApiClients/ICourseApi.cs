using Refit;
using SIS.BLL.Services.DTOs.Course;
using SIS.Common.Pagination;

namespace SIS.UI.ApiClients
{
    public interface ICourseApi
    {
        [Get("/api/course")]
        Task<PagedResult<CourseListDto>> GetAllAsync([Query] PagedRequest request);

        [Get("/api/course/{id}")]
        Task<CourseDetailDto> GetByIdAsync(int id);

        [Get("/api/course/department/{departmentId}")]
        Task<IEnumerable<CourseListDto>> GetByDepartmentAsync(int departmentId);

        [Post("/api/course")]
        Task CreateAsync([Body] CreateCourseDto dto);

        [Put("/api/course")]
        Task UpdateAsync([Body] UpdateCourseDto dto);

        [Delete("/api/course/{id}")]
        Task DeleteAsync(int id);
    }


}
