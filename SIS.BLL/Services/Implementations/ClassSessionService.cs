using AutoMapper;
using SIS.BLL.Helpers;
using SIS.BLL.Services.DTOs.ClassSession;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class ClassSessionService : IClassSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IActivityLogger _activityLogger;
        private readonly IClock _clock;

        public ClassSessionService(
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

        public async Task<ClassSessionDto?> GetByIdAsync(int id)
        {
            var session = await _unitOfWork.ClassSessions.GetWithDetailsAsync(id);
            return _mapper.Map<ClassSessionDto>(session);
        }

        public async Task<IEnumerable<ClassSessionDto>> GetByCourseOfferingAsync(int courseOfferingId)
        {
            var sessions = await _unitOfWork.ClassSessions.GetByCourseOfferingAsync(courseOfferingId);
            return _mapper.Map<IEnumerable<ClassSessionDto>>(sessions);
        }

        public async Task<IEnumerable<ClassSessionDto>> GetByCourseOfferingAndRangeAsync(
            int courseOfferingId, DateOnly from, DateOnly to)
        {
            var sessions = await _unitOfWork.ClassSessions
                .GetByCourseOfferingAndDateRangeAsync(courseOfferingId, from, to);
            return _mapper.Map<IEnumerable<ClassSessionDto>>(sessions);
        }

        public async Task<IEnumerable<ClassSessionDto>> GetByStudentAndRangeAsync(
            int studentId, DateOnly from, DateOnly to)
        {
            var sessions = await _unitOfWork.ClassSessions
                .GetByStudentAndDateRangeAsync(studentId, from, to);
            return _mapper.Map<IEnumerable<ClassSessionDto>>(sessions);
        }

        public async Task<int> GenerateFromWeeklyScheduleAsync(int courseOfferingId)
        {
            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(courseOfferingId);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            TermPhaseGuard.EnsurePhase(_clock, offering.AcademicTerm,
                "Sessiya generasiyası",
                TermPhase.Planning, TermPhase.Registration,
                TermPhase.Preparation, TermPhase.Active);

            var patterns = (await _unitOfWork.WeeklySchedulePatterns
                .GetByCourseOfferingAsync(courseOfferingId)).ToList();

            if (patterns.Count == 0)
                throw new BadRequestException(
                    "Bu kurs üçün həftəlik cədvəl təyin olunmayıb. Əvvəlcə cədvəl qurun.");

            var termStart = DateOnly.FromDateTime(offering.AcademicTerm.StartDate);
            var termEnd = DateOnly.FromDateTime(offering.AcademicTerm.EndDate);

            // Term aralığındakı mövcud sessiyaları yüklə (təkrar yaratmamaq üçün)
            var existing = (await _unitOfWork.ClassSessions
                .GetByCourseOfferingAndDateRangeAsync(courseOfferingId, termStart, termEnd))
                .Select(s => new { s.Date, s.StartTime })
                .ToHashSet();

            var created = 0;
            for (var date = termStart; date <= termEnd; date = date.AddDays(1))
            {
                var todayPatterns = patterns.Where(p => p.DayOfWeek == date.DayOfWeek);
                foreach (var p in todayPatterns)
                {
                    if (existing.Contains(new { Date = date, p.StartTime }))
                        continue;

                    var session = new ClassSession
                    {
                        CourseOfferingId = courseOfferingId,
                        Date = date,
                        StartTime = p.StartTime,
                        EndTime = p.EndTime,
                        Room = p.Room ?? offering.Room,
                        Status = ClassSessionStatus.Scheduled
                    };
                    await _unitOfWork.ClassSessions.AddAsync(session);
                    created++;
                }
            }

            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Generate", "ClassSession",
                courseOfferingId.ToString(), null,
                new { CreatedCount = created, From = termStart, To = termEnd });

            return created;
        }

        public async Task<int> CreateAsync(CreateClassSessionDto dto)
        {
            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(dto.CourseOfferingId);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            TermPhaseGuard.EnsurePhase(_clock, offering.AcademicTerm,
                "Sessiya yaradılması",
                TermPhase.Planning, TermPhase.Registration,
                TermPhase.Preparation, TermPhase.Active);

            EnsureDateWithinTerm(offering.AcademicTerm, dto.Date);

            var exists = await _unitOfWork.ClassSessions
                .ExistsAsync(dto.CourseOfferingId, dto.Date, dto.StartTime);
            if (exists)
                throw new ConflictException("Bu tarix və başlama saatına aid sessiya artıq var");

            var session = _mapper.Map<ClassSession>(dto);
            session.Status = ClassSessionStatus.Scheduled;
            await _unitOfWork.ClassSessions.AddAsync(session);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Create", "ClassSession", session.Id.ToString(),
                null, new { dto.CourseOfferingId, dto.Date, dto.StartTime });

            return session.Id;
        }

        public async Task UpdateAsync(UpdateClassSessionDto dto)
        {
            var session = await _unitOfWork.ClassSessions.GetByIdAsync(dto.Id);
            if (session == null)
                throw new NotFoundException("Sessiya tapılmadı");

            if (session.Status == ClassSessionStatus.Cancelled)
                throw new BadRequestException("Ləğv olunmuş sessiyanı redaktə etmək olmaz");

            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(session.CourseOfferingId);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            TermPhaseGuard.EnsurePhase(_clock, offering.AcademicTerm,
                "Sessiya redaktəsi",
                TermPhase.Planning, TermPhase.Registration,
                TermPhase.Preparation, TermPhase.Active);

            EnsureDateWithinTerm(offering.AcademicTerm, dto.Date);

            var oldValue = new { session.Date, session.StartTime, session.Topic };

            session.Date = dto.Date;
            session.StartTime = dto.StartTime;
            session.EndTime = dto.EndTime;
            session.Room = dto.Room;
            session.Topic = dto.Topic;
            session.Notes = dto.Notes;
            session.UpdatedAt = _clock.UtcNow;

            _unitOfWork.ClassSessions.Update(session);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Update", "ClassSession", dto.Id.ToString(),
                oldValue, new { dto.Date, dto.StartTime, dto.Topic });
        }

        public async Task CancelAsync(CancelClassSessionDto dto)
        {
            var session = await _unitOfWork.ClassSessions.GetByIdAsync(dto.Id);
            if (session == null)
                throw new NotFoundException("Sessiya tapılmadı");

            if (session.Status == ClassSessionStatus.Cancelled)
                throw new BadRequestException("Sessiya artıq ləğv olunub");
            if (session.Status == ClassSessionStatus.Completed)
                throw new BadRequestException("Keçirilmiş sessiyanı ləğv etmək olmaz");

            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(session.CourseOfferingId);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            TermPhaseGuard.EnsurePhase(_clock, offering.AcademicTerm,
                "Sessiya ləğvi",
                TermPhase.Preparation, TermPhase.Active);

            session.Status = ClassSessionStatus.Cancelled;
            session.CancellationReason = dto.Reason;
            session.UpdatedAt = _clock.UtcNow;

            _unitOfWork.ClassSessions.Update(session);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Cancel", "ClassSession", dto.Id.ToString(),
                null, new { dto.Reason });
        }

        public async Task<int> CreateMakeupAsync(CreateMakeupSessionDto dto)
        {
            var original = await _unitOfWork.ClassSessions.GetByIdAsync(dto.OriginalSessionId);
            if (original == null)
                throw new NotFoundException("Əvəz olunan sessiya tapılmadı");
            if (original.Status != ClassSessionStatus.Cancelled)
                throw new BadRequestException("Yalnız ləğv olunmuş sessiyanın makeup-ı ola bilər");

            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(original.CourseOfferingId);
            if (offering == null)
                throw new NotFoundException("Kurs təklifi tapılmadı");

            TermPhaseGuard.EnsurePhase(_clock, offering.AcademicTerm,
                "Makeup sessiyası",
                TermPhase.Preparation, TermPhase.Active);

            EnsureDateWithinTerm(offering.AcademicTerm, dto.Date);

            var exists = await _unitOfWork.ClassSessions
                .ExistsAsync(original.CourseOfferingId, dto.Date, dto.StartTime);
            if (exists)
                throw new ConflictException("Bu tarix və başlama saatına aid sessiya artıq var");

            var makeup = new ClassSession
            {
                CourseOfferingId = original.CourseOfferingId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Room = dto.Room ?? offering.Room,
                Status = ClassSessionStatus.Makeup,
                Topic = dto.Topic,
                OriginalSessionId = original.Id
            };
            await _unitOfWork.ClassSessions.AddAsync(makeup);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("CreateMakeup", "ClassSession", makeup.Id.ToString(),
                null, new { OriginalId = original.Id, dto.Date, dto.StartTime });

            return makeup.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var session = await _unitOfWork.ClassSessions.GetByIdAsync(id);
            if (session == null)
                throw new NotFoundException("Sessiya tapılmadı");

            if (session.Status == ClassSessionStatus.Completed)
                throw new BadRequestException("Keçirilmiş sessiyanı silmək olmaz (audit üçün)");

            _unitOfWork.ClassSessions.SoftDelete(session);
            await _unitOfWork.SaveAsync();

            await _activityLogger.LogAsync("Delete", "ClassSession", id.ToString(),
                new { session.Date, session.StartTime }, null);
        }

        private static void EnsureDateWithinTerm(AcademicTerm term, DateOnly date)
        {
            var termStart = DateOnly.FromDateTime(term.StartDate);
            var termEnd = DateOnly.FromDateTime(term.EndDate);
            if (date < termStart || date > termEnd)
                throw new BadRequestException(
                    $"Sessiya tarixi semestr aralığında olmalıdır ({termStart} – {termEnd})");
        }
    }
}
