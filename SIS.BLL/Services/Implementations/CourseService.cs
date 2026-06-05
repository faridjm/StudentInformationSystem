using SIS.BLL.Services.DTOs.Course;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<IEnumerable<CourseDto>> GetAllAsync()
        {
            var courses = await _unitOfWork.Courses.GetAllWithGroupsAsync();
            return courses.Select(c => new CourseDto
            {
                Id = c.Id, Name = c.Name, Description = c.Description,
                IsActive = c.IsActive, GroupCount = c.Groups?.Count ?? 0,
                Credit = c.Credit, TotalHours = c.TotalHours, Level = c.Level,
                DepartmentId = c.DepartmentId, DepartmentName = c.Department?.Name
            });
        }

        public async Task<CourseDto?> GetByIdAsync(int id)
        {
            var c = await _unitOfWork.Courses.GetByIdAsync(id);
            if (c == null) return null;
            return new CourseDto { Id = c.Id, Name = c.Name, Description = c.Description, IsActive = c.IsActive,
                Credit = c.Credit, TotalHours = c.TotalHours, Level = c.Level, DepartmentId = c.DepartmentId };
        }

        public async Task<CourseDto> CreateAsync(CreateCourseDto dto)
        {
            var course = new Course { Name = dto.Name, Description = dto.Description, CreatedAt = DateTime.UtcNow,
                Credit = dto.Credit, TotalHours = dto.TotalHours, Level = dto.Level, DepartmentId = dto.DepartmentId };
            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveAsync();
            return new CourseDto { Id = course.Id, Name = course.Name, Description = course.Description, IsActive = course.IsActive,
                Credit = course.Credit, TotalHours = course.TotalHours, Level = course.Level };
        }

        public async Task UpdateAsync(UpdateCourseDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Kurs tapılmadı");
            course.Name = dto.Name;
            course.Description = dto.Description;
            course.IsActive = dto.IsActive;
            course.Credit = dto.Credit;
            course.TotalHours = dto.TotalHours;
            course.Level = dto.Level;
            course.DepartmentId = dto.DepartmentId;
            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id)
                ?? throw new NotFoundException("Kurs tapılmadı");
            _unitOfWork.Courses.SoftDelete(course);
            await _unitOfWork.SaveAsync();
        }
    }
}
