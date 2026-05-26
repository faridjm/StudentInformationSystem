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

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clock = clock;
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

            // Bütün dtos eyni Enrollment-lərə aiddir, amma müxtəlif gün ola bilər.
            // Birinci dto-nun enrollment-i üzərindən Term-i yoxlayırıq (hamısı eyni offering-ə aid olmalıdır).
            await EnsureAttendanceAllowedAsync(dtoList[0].EnrollmentId, dtoList[0].SessionDate);

            foreach (var dto in dtoList)
            {
                var existing = await _unitOfWork.AttendanceRecords.GetAsync(
                    a => a.EnrollmentId == dto.EnrollmentId
                      && a.SessionDate.Date == dto.SessionDate.Date);

                if (existing != null)
                {
                    existing.Status = dto.Status;
                    existing.Note = dto.Note;
                    _unitOfWork.AttendanceRecords.Update(existing);
                }
                else
                {
                    var record = _mapper.Map<AttendanceRecord>(dto);
                    await _unitOfWork.AttendanceRecords.AddAsync(record);
                }
            }
            await _unitOfWork.SaveAsync();
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

            TermPhaseGuard.EnsurePhase(_clock, enrollment.CourseOffering.AcademicTerm,
                "Davamiyyət qeydi",
                TermPhase.Active, TermPhase.ExamPeriod);

            if (sessionDate.Date > _clock.UtcNow.Date)
                throw new BadRequestException("Sessiya tarixi gələcəkdə ola bilməz");
        }
    }
}
