using SIS.BLL.Services.DTOs.Course;

namespace SIS.BLL.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseListDto>> GetAllCoursesAsync();
        Task<CourseDetailDto?> GetCourseByIdAsync(int id);
        Task<IEnumerable<CourseListDto>> GetCoursesByDepartmentAsync(int departmentId);
        Task CreateCourseAsync(CreateCourseDto dto);
        Task UpdateCourseAsync(UpdateCourseDto dto);
        Task DeleteCourseAsync(int id);
    }
}
