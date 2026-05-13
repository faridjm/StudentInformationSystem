using SIS.BLL.Services.DTOs.Course;
using SIS.Common.Pagination;

namespace SIS.BLL.Services.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResult<CourseListDto>> GetAllCoursesAsync(PagedRequest request);
        Task<CourseDetailDto?> GetCourseByIdAsync(int id);
        Task<IEnumerable<CourseListDto>> GetCoursesByDepartmentAsync(int departmentId);
        Task CreateCourseAsync(CreateCourseDto dto);
        Task UpdateCourseAsync(UpdateCourseDto dto);
        Task DeleteCourseAsync(int id);
    }
}
