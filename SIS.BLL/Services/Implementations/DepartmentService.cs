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
            var trimmedName = (dto.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new BadRequestException("Şöbə adı boş ola bilməz");

            // Duplicate name yoxlaması (case-insensitive + trim)
            await EnsureNameUniqueAsync(trimmedName, excludeId: null);

            var department = new Department
            {
                Name = trimmedName,
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

            var trimmedName = (dto.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new BadRequestException("Şöbə adı boş ola bilməz");

            // Duplicate name yoxlaması (öz Id-si istisna)
            await EnsureNameUniqueAsync(trimmedName, excludeId: dto.Id);

            department.Name = trimmedName;
            department.Description = dto.Description;
            department.IsActive = dto.IsActive;
            department.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Eyni ad ilə başqa aktiv (silinməmiş) department varsa BadRequestException atır.
        /// SQL Server-in default CI collation-u case-insensitive comparison edir.
        /// Trim C# tərəfdə tətbiq olunur, DB-dən tam siyahı çəkilmir — EF.Functions.Like istifadə olunur.
        /// </summary>
        private async Task EnsureNameUniqueAsync(string name, int? excludeId)
        {
            // SQL Server default collation CI-dir, ona görə "=" yetərlidir.
            // Lakin başda/sonda boşluq ola bilər — bunu da yoxlayaq.
            var trimmed = name.Trim();

            // Soft-deleted olanları da istisna et
            var allDepts = await _unitOfWork.Departments.GetAllAsync(d => !d.IsDeleted);

            var duplicate = allDepts.FirstOrDefault(d =>
                d.Name != null &&
                string.Equals(d.Name.Trim(), trimmed, StringComparison.OrdinalIgnoreCase) &&
                d.Id != (excludeId ?? 0));

            if (duplicate != null)
                throw new BadRequestException("Bu adda şöbə artıq mövcuddur.");
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
