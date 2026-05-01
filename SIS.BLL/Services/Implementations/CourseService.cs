using AutoMapper;
using SIS.BLL.Services.DTOs.Course;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseListDto>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.Courses.GetAllAsync();
            return _mapper.Map<IEnumerable<CourseListDto>>(courses);
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
            var isUnique = await _unitOfWork.Courses.IsCourseCodeUniqueAsync(dto.Code);
            if (!isUnique)
                throw new Exception($"{dto.Code} kodu ilə kurs artıq mövcuddur");

            var course = _mapper.Map<Course>(dto);
            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateCourseAsync(UpdateCourseDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(dto.Id);
            if (course == null)
                throw new Exception("Kurs tapılmadı");

            _mapper.Map(dto, course);
            course.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                throw new Exception("Kurs tapılmadı");

            _unitOfWork.Courses.SoftDelete(course);
            await _unitOfWork.SaveAsync();
        }
    }
}
