using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IActivityLogger _activityLogger;

        public AcademicYearService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IActivityLogger activityLogger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _activityLogger = activityLogger;
        }

        public async Task<IEnumerable<AcademicYearDto>> GetAllYearsAsync()
        {
            var years = await _unitOfWork.AcademicYears.GetAllWithTermsAsync();
            return _mapper.Map<IEnumerable<AcademicYearDto>>(years);
        }

        public async Task<AcademicYearDto?> GetYearByIdAsync(int id)
        {
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            return _mapper.Map<AcademicYearDto>(year);
        }

        public async Task<AcademicYearDto?> GetActiveYearAsync()
        {
            var year = await _unitOfWork.AcademicYears.GetActiveYearAsync();
            return _mapper.Map<AcademicYearDto>(year);
        }

        public async Task CreateYearAsync(CreateAcademicYearDto dto)
        {
            // Eyni adlı sezon olmamalıdır
            var existing = await _unitOfWork.AcademicYears.GetByNameAsync(dto.Name);
            if (existing != null)
                throw new ConflictException($"\"{dto.Name}\" adlı dərs sezonu artıq mövcuddur");

            // Üst-üstə düşən tarix aralığı olmamalıdır
            var overlapping = await _unitOfWork.AcademicYears
                .GetAllAsync(y => dto.StartDate <= y.EndDate && dto.EndDate >= y.StartDate);
            if (overlapping.Any())
                throw new ConflictException("Bu tarix aralığı başqa sezonla üst-üstə düşür");

            var year = _mapper.Map<AcademicYear>(dto);
            await _unitOfWork.AcademicYears.AddAsync(year);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Create", "AcademicYear", year.Id.ToString(),
                null, new { dto.Name, dto.StartDate, dto.EndDate });
        }

        public async Task UpdateYearAsync(UpdateAcademicYearDto dto)
        {
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(dto.Id);
            if (year == null)
                throw new NotFoundException("Dərs sezonu tapılmadı");

            // Ad dəyişirsə, eyni adlı başqa sezon olmamalıdır
            if (year.Name != dto.Name)
            {
                var existing = await _unitOfWork.AcademicYears.GetByNameAsync(dto.Name);
                if (existing != null && existing.Id != dto.Id)
                    throw new ConflictException($"\"{dto.Name}\" adlı dərs sezonu artıq mövcuddur");
            }

            var oldValue = new { year.Name, year.StartDate, year.EndDate };

            _mapper.Map(dto, year);
            year.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.AcademicYears.Update(year);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Update", "AcademicYear", dto.Id.ToString(),
                oldValue, new { dto.Name, dto.StartDate, dto.EndDate });
        }

        public async Task SetActiveYearAsync(int id)
        {
            var allYears = await _unitOfWork.AcademicYears.GetAllAsync();
            foreach (var y in allYears)
            {
                if (y.IsActive)
                {
                    y.IsActive = false;
                    _unitOfWork.AcademicYears.Update(y);
                }
            }

            var year = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            if (year == null)
                throw new NotFoundException("Dərs sezonu tapılmadı");

            year.IsActive = true;
            year.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.AcademicYears.Update(year);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("SetActive", "AcademicYear", id.ToString(),
                null, new { year.Name });
        }

        public async Task DeleteYearAsync(int id)
        {
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            if (year == null)
                throw new NotFoundException("Dərs sezonu tapılmadı");

            // Bağlı semestrlər varsa silmək olmaz
            var hasTerms = await _unitOfWork.AcademicTerms.AnyAsync(t => t.AcademicYearId == id);
            if (hasTerms)
                throw new ConflictException("Bu sezona bağlı semestrlər var. Əvvəl onları silin.");

            _unitOfWork.AcademicYears.SoftDelete(year);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Delete", "AcademicYear", id.ToString(),
                new { year.Name }, null);
        }
    }
}
