using SIS.BLL.Services.DTOs.Department;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
        {
            var departments = await _unitOfWork.Departments.GetAllWithCoursesAsync();

            // Aktiv kurs sayını birbaşa Course cədvəlindən oxuyuruq —
            // bu navigation include-dan daha güvənilirdir (silinmiş kurslar avtomatik filtrlənir).
            var activeCourses = await _unitOfWork.Courses.GetAllAsync(c => c.IsActive && c.DepartmentId.HasValue);
            var countByDept = activeCourses
                .GroupBy(c => c.DepartmentId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                ActiveCourseCount = countByDept.TryGetValue(d.Id, out var cnt) ? cnt : 0
            });
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var d = await _unitOfWork.Departments.GetByIdWithCoursesAsync(id);
            if (d == null) return null;

            var count = await _unitOfWork.Courses.CountAsync(c =>
                c.IsActive && c.DepartmentId == id);

            return new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                ActiveCourseCount = count
            };
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveAsync();
            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                ActiveCourseCount = 0
            };
        }

        public async Task UpdateAsync(UpdateDepartmentDto dto)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Şöbə tapılmadı");
            department.Name = dto.Name;
            department.Description = dto.Description;
            department.IsActive = dto.IsActive;
            department.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveAsync();
        }

        public async Task SetActiveAsync(int id, bool isActive)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id)
                ?? throw new NotFoundException("Şöbə tapılmadı");
            department.IsActive = isActive;
            department.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id)
                ?? throw new NotFoundException("Şöbə tapılmadı");
            _unitOfWork.Departments.SoftDelete(department);
            await _unitOfWork.SaveAsync();
        }
    }
}
