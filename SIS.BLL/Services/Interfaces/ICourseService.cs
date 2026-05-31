using SIS.BLL.Services.DTOs.Course;

namespace SIS.BLL.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllAsync();
        Task<CourseDto?> GetByIdAsync(int id);
        Task<CourseDto> CreateAsync(CreateCourseDto dto);
        Task UpdateAsync(UpdateCourseDto dto);
        Task DeleteAsync(int id);
    }
}
