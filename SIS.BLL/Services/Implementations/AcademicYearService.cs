using SIS.BLL.Services.DTOs.AcademicYear;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActivityLogger _activityLogger;

        public AcademicYearService(IUnitOfWork unitOfWork, IActivityLogger activityLogger)
        {
            _unitOfWork = unitOfWork;
            _activityLogger = activityLogger;
        }

        public async Task<IEnumerable<AcademicYearDto>> GetAllYearsAsync()
        {
            var years = await _unitOfWork.AcademicYears.GetAllAsync();
            return years.Select(MapToDto);
        }

        public async Task<AcademicYearDto?> GetYearByIdAsync(int id)
        {
            var y = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            return y == null ? null : MapToDto(y);
        }

        public async Task<AcademicYearDto?> GetActiveYearAsync()
        {
            var y = await _unitOfWork.AcademicYears.GetActiveYearAsync();
            return y == null ? null : MapToDto(y);
        }

        public async Task CreateYearAsync(CreateAcademicYearDto dto)
        {
            var existing = await _unitOfWork.AcademicYears.GetByNameAsync(dto.Name);
            if (existing != null)
                throw new ConflictException($"\"{dto.Name}\" adlı sezon artıq mövcuddur");

            var year = new AcademicYear
            {
                Name = dto.Name, StartDate = dto.StartDate,
                EndDate = dto.EndDate, CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AcademicYears.AddAsync(year);
            await _unitOfWork.SaveAsync();
            await _activityLogger.LogAsync("Create", "AcademicYear", year.Id.ToString(), null, new { dto.Name });
        }

        public async Task UpdateYearAsync(UpdateAcademicYearDto dto)
        {
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Sezon tapılmadı");

            year.Name = dto.Name; year.StartDate = dto.StartDate;
            year.EndDate = dto.EndDate; year.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.AcademicYears.Update(year);
            await _unitOfWork.SaveAsync();
        }

        public async Task SetActiveYearAsync(int id)
        {
            var all = await _unitOfWork.AcademicYears.GetAllAsync();
            foreach (var y in all.Where(y => y.IsActive))
            {
                y.IsActive = false;
                _unitOfWork.AcademicYears.Update(y);
            }
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(id)
                ?? throw new NotFoundException("Sezon tapılmadı");
            year.IsActive = true; year.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.AcademicYears.Update(year);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteYearAsync(int id)
        {
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(id)
                ?? throw new NotFoundException("Sezon tapılmadı");
            _unitOfWork.AcademicYears.SoftDelete(year);
            await _unitOfWork.SaveAsync();
        }

        private static AcademicYearDto MapToDto(AcademicYear y) => new()
        {
            Id = y.Id, Name = y.Name, StartDate = y.StartDate,
            EndDate = y.EndDate, IsActive = y.IsActive,
            IsCurrent = y.StartDate <= DateTime.UtcNow && DateTime.UtcNow <= y.EndDate
        };
    }
}
