using AutoMapper;
using SIS.BLL.Helpers;
using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper, IClock clock, ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clock = clock;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByEnrollmentAsync(int enrollmentId)
        {
            var records = await _unitOfWork.AttendanceRecords.GetAttendanceByEnrollmentAsync(enrollmentId);
            return _mapper.Map<IEnumerable<AttendanceDto>>(records);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByDateAsync(int courseOfferingId, DateTime date)
        {
            var records = await _unitOfWork.AttendanceRecords.GetAttendanceByDateAsync(courseOfferingId, date);
            return _mapper.Map<IEnumerable<AttendanceDto>>(records);
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int enrollmentId)
        {
            var records = await _unitOfWork.AttendanceRecords.GetAttendanceByEnrollmentAsync(enrollmentId);
            var list = records.ToList();

            return new AttendanceSummaryDto
            {
                TotalSessions = list.Count,
                PresentCount = list.Count(r => r.Status == AttendanceStatus.Present),
                AbsentCount = list.Count(r => r.Status == AttendanceStatus.Absent),
                LateCount = list.Count(r => r.Status == AttendanceStatus.Late),
                AttendancePercentage = await _unitOfWork.AttendanceRecords.GetAttendancePercentageAsync(enrollmentId)
            };
        }

        public async Task RecordAttendanceAsync(CreateAttendanceDto dto)
        {
            await EnsureAttendanceAllowedAsync(dto.EnrollmentId, dto.SessionDate);

            var record = _mapper.Map<AttendanceRecord>(dto);
            await _unitOfWork.AttendanceRecords.AddAsync(record);
            await _unitOfWork.SaveAsync();
        }

        public async Task RecordBulkAttendanceAsync(IEnumerable<CreateAttendanceDto> dtos)
        {
            var dtoList = dtos.ToList();
            if (dtoList.Count == 0) return;

            await EnsureAttendanceAllowedAsync(dtoList[0].EnrollmentId, dtoList[0].SessionDate);

            // ClassSessionId varsa, onun mövcudluğunu yoxla və tarixi sessiyadan al
            if (dtoList[0].ClassSessionId.HasValue)
            {
                var session = await _unitOfWork.ClassSessions.GetByIdAsync(dtoList[0].ClassSessionId.Value);
                if (session == null)
                    throw new NotFoundException("Seçilmiş dərs sessiyası tapılmadı");

                if (session.Status == ClassSessionStatus.Cancelled)
                    throw new BadRequestException("Ləğv olunmuş sessiya üçün davamiyyət qeyd edilə bilməz");

                // Hər bir dto üçün SessionDate-i sessiyadan ayarla (consistency üçün)
                foreach (var dto in dtoList)
                {
                    dto.SessionDate = session.Date.ToDateTime(TimeOnly.MinValue);
                }
            }

            foreach (var dto in dtoList)
            {
                // ClassSessionId varsa, ona görə tapırıq; yoxdursa köhnə üsulla SessionDate-ə görə
                AttendanceRecord? existing = null;
                if (dto.ClassSessionId.HasValue)
                {
                    existing = await _unitOfWork.AttendanceRecords.GetAsync(
                        a => a.EnrollmentId == dto.EnrollmentId
                          && a.ClassSessionId == dto.ClassSessionId);
                }
                else
                {
                    existing = await _unitOfWork.AttendanceRecords.GetAsync(
                        a => a.EnrollmentId == dto.EnrollmentId
                          && a.SessionDate.Date == dto.SessionDate.Date);
                }

                if (existing != null)
                {
                    existing.Status = dto.Status;
                    existing.Note = dto.Note;
                    if (dto.ClassSessionId.HasValue)
                        existing.ClassSessionId = dto.ClassSessionId;
                    _unitOfWork.AttendanceRecords.Update(existing);
                }
                else
                {
                    var record = _mapper.Map<AttendanceRecord>(dto);
                    await _unitOfWork.AttendanceRecords.AddAsync(record);
                }
            }
            await _unitOfWork.SaveAsync();

            // Sessiya statusunu Scheduled → Completed yenilə (əgər keçmişdə deyildirsə)
            if (dtoList[0].ClassSessionId.HasValue)
            {
                var session = await _unitOfWork.ClassSessions.GetByIdAsync(dtoList[0].ClassSessionId.Value);
                if (session != null && session.Status == ClassSessionStatus.Scheduled)
                {
                    session.Status = ClassSessionStatus.Completed;
                    session.UpdatedAt = _clock.UtcNow;
                    _unitOfWork.ClassSessions.Update(session);
                    await _unitOfWork.SaveAsync();
                }
            }
        }

        /// <summary>
        /// Davamiyyət qeydi üçün şərtlər:
        /// 1. Termin fazası Active və ya ExamPeriod olmalıdır
        /// 2. Sessiya tarixi gələcəkdə ola bilməz
        /// </summary>
        private async Task EnsureAttendanceAllowedAsync(int enrollmentId, DateTime sessionDate)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId);
            if (enrollment == null)
                throw new NotFoundException("Qeydiyyat tapılmadı");

            if (!_currentUser.CanBypassPhaseRules)
            {
                TermPhaseGuard.EnsurePhase(_clock, enrollment.CourseOffering.AcademicTerm,
                    "Davamiyyət qeydi",
                    TermPhase.Active, TermPhase.ExamPeriod);

                if (sessionDate.Date > _clock.UtcNow.Date)
                    throw new BadRequestException("Sessiya tarixi gələcəkdə ola bilməz");
            }
        }
    }
}
