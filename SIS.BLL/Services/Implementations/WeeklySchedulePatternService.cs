using AutoMapper;
using SIS.BLL.Helpers;
using SIS.BLL.Services.DTOs.WeeklySchedule;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class WeeklySchedulePatternService : IWeeklySchedulePatternService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IActivityLogger _activityLogger;
        private readonly IClock _clock;

        public WeeklySchedulePatternService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IActivityLogger activityLogger,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _activityLogger = activityLogger;
            _clock = clock;
        }

        public async Task<IEnumerable<WeeklySchedulePatternDto>> GetByCourseOfferingAsync(int courseOfferingId)
        {
            var patterns = await _unitOfWork.WeeklySchedulePatterns
                .GetByCourseOfferingAsync(courseOfferingId);
            return _mapper.Map<IEnumerable<WeeklySchedulePatternDto>>(patterns);
        }

        public async Task SetScheduleAsync(SetWeeklyScheduleDto dto)
        {
            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(dto.CourseOfferingId);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            // Müəllim cədvəli yalnız semestr başlamamış / dərs gedirkən redaktə edə bilər
            TermPhaseGuard.EnsurePhase(_clock, offering.AcademicTerm,
                "Həftəlik cədvəl redaktəsi",
                TermPhase.Planning, TermPhase.Registration,
                TermPhase.Preparation, TermPhase.Active);

            // Mövcud şablonları soft-delete
            await _unitOfWork.WeeklySchedulePatterns.DeleteByCourseOfferingAsync(dto.CourseOfferingId);

            // Yenilərini əlavə et
            foreach (var slot in dto.Slots)
            {
                var pattern = new WeeklySchedulePattern
                {
                    CourseOfferingId = dto.CourseOfferingId,
                    DayOfWeek = slot.DayOfWeek,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Room = slot.Room
                };
                await _unitOfWork.WeeklySchedulePatterns.AddAsync(pattern);
            }

            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Set", "WeeklySchedule",
                dto.CourseOfferingId.ToString(),
                null,
                new { dto.CourseOfferingId, SlotCount = dto.Slots.Count });
        }

        public async Task ClearScheduleAsync(int courseOfferingId)
        {
            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(courseOfferingId);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            TermPhaseGuard.EnsurePhase(_clock, offering.AcademicTerm,
                "Həftəlik cədvəlin silinməsi",
                TermPhase.Planning, TermPhase.Registration,
                TermPhase.Preparation);

            await _unitOfWork.WeeklySchedulePatterns.DeleteByCourseOfferingAsync(courseOfferingId);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Clear", "WeeklySchedule",
                courseOfferingId.ToString(), null, null);
        }
    }
}
