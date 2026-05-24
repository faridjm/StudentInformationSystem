using AutoMapper;
using SIS.BLL.Services.DTOs.Course;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Pagination;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IActivityLogger _activityLogger;

        public CourseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IActivityLogger activityLogger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _activityLogger = activityLogger;
        }

        public async Task<PagedResult<CourseListDto>> GetAllCoursesAsync(PagedRequest request)
        {
            var pagedCourses = await _unitOfWork.Courses.GetPagedAsync(request);

            return new PagedResult<CourseListDto>
            {
                Items = _mapper.Map<IEnumerable<CourseListDto>>(pagedCourses.Items),
                TotalCount = pagedCourses.TotalCount,
                Page = pagedCourses.Page,
                PageSize = pagedCourses.PageSize
            };
        }

        public async Task<CourseDetailDto?> GetCourseByIdAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetCourseWithDetailsAsync(id);
            return _mapper.Map<CourseDetailDto>(course);
        }

        public async Task<IEnumerable<CourseListDto>> GetCoursesByDepartmentAsync(int departmentId)
        {
            var courses = await _unitOfWork.Courses.GetCoursesByDepartmentAsync(departmentId);
            return _mapper.Map<IEnumerable<CourseListDto>>(courses);
        }

        public async Task CreateCourseAsync(CreateCourseDto dto)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
            if (department == null)
                throw new NotFoundException("Departament tapılmadı");

            var isUnique = await _unitOfWork.Courses.IsCourseCodeUniqueAsync(dto.Code);
            if (!isUnique)
                throw new ConflictException($"{dto.Code} kodu ilə kurs artıq mövcuddur");

            var course = _mapper.Map<Course>(dto);
            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Create", "Course", course.Id.ToString(),
                null, new { dto.Name, dto.Code });
        }

        public async Task UpdateCourseAsync(UpdateCourseDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(dto.Id);
            if (course == null)
                throw new NotFoundException("Kurs tapılmadı");

            var oldValue = new { course.Name, course.Code, course.Credits };

            _mapper.Map(dto, course);
            course.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Update", "Course", dto.Id.ToString(),
                oldValue, new { dto.Name, dto.Credits });
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                throw new NotFoundException("Kurs tapılmadı");

            _unitOfWork.Courses.SoftDelete(course);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Delete", "Course", id.ToString(),
                new { course.Name, course.Code }, null);
        }
    }
}
