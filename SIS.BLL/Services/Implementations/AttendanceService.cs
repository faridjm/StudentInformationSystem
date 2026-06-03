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
        private readonly IClock _clock;
        private readonly IAuditLogService _audit;

        public AttendanceService(IUnitOfWork unitOfWork, IClock clock, IAuditLogService audit)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _audit = audit;
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByEnrollmentAsync(int enrollmentId)
        {
            var records = await _unitOfWork.AttendanceRecords.GetAttendanceByEnrollmentAsync(enrollmentId);
            return records.Select(MapToDto);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceBySessionAsync(int sessionId)
        {
            var records = await _unitOfWork.AttendanceRecords.GetAttendanceBySessionAsync(sessionId);
            return records.Select(MapToDto);
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int enrollmentId)
        {
            var records = (await _unitOfWork.AttendanceRecords.GetAttendanceByEnrollmentAsync(enrollmentId)).ToList();
            return new AttendanceSummaryDto
            {
                TotalSessions = records.Count,
                PresentCount = records.Count(r => r.Status == AttendanceStatus.Present),
                AbsentCount = records.Count(r => r.Status == AttendanceStatus.Absent),
                LateCount = records.Count(r => r.Status == AttendanceStatus.Late),
                AttendancePercentage = await _unitOfWork.AttendanceRecords.GetAttendancePercentageAsync(enrollmentId)
            };
        }

        public async Task RecordBulkAttendanceAsync(IEnumerable<CreateAttendanceDto> dtos)
        {
            var dtoList = dtos.ToList();
            if (dtoList.Count == 0) return;

            int? sessionId = dtoList[0].ClassSessionId;
            ClassSession? session = null;

            if (sessionId.HasValue)
            {
                session = await _unitOfWork.ClassSessions.GetByIdAsync(sessionId.Value)
                    ?? throw new NotFoundException("Dərs sessiyası tapılmadı");

                if (session.Status == ClassSessionStatus.Cancelled)
                    throw new BadRequestException("Ləğv olunmuş sessiya üçün davamiyyət qeyd edilə bilməz");

                foreach (var d in dtoList)
                    d.SessionDate = session.Date.ToDateTime(TimeOnly.MinValue);
            }

            foreach (var dto in dtoList)
            {
                AttendanceRecord? existing = null;
                if (dto.ClassSessionId.HasValue)
                    existing = await _unitOfWork.AttendanceRecords.GetAsync(
                        a => a.EnrollmentId == dto.EnrollmentId && a.ClassSessionId == dto.ClassSessionId);
                else
                    existing = await _unitOfWork.AttendanceRecords.GetAsync(
                        a => a.EnrollmentId == dto.EnrollmentId && a.SessionDate.Date == dto.SessionDate.Date);

                if (existing != null)
                {
                    existing.Status = dto.Status;
                    existing.Note = dto.Note;
                    _unitOfWork.AttendanceRecords.Update(existing);
                }
                else
                {
                    await _unitOfWork.AttendanceRecords.AddAsync(new AttendanceRecord
                    {
                        EnrollmentId = dto.EnrollmentId,
                        ClassSessionId = dto.ClassSessionId,
                        SessionDate = dto.SessionDate,
                        Status = dto.Status,
                        Note = dto.Note,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Save", "Attendance", sessionId?.ToString(),
                $"Davamiyyət qeyd edildi: {dtoList.Count} tələbə" + (sessionId.HasValue ? $" (Session #{sessionId})" : ""));

            if (session != null && session.Status == ClassSessionStatus.Scheduled)
            {
                session.Status = ClassSessionStatus.Completed;
                session.UpdatedAt = _clock.UtcNow;
                _unitOfWork.ClassSessions.Update(session);
                await _unitOfWork.SaveAsync();
            }
        }

        private static AttendanceDto MapToDto(AttendanceRecord a) => new()
        {
            Id = a.Id, EnrollmentId = a.EnrollmentId,
            StudentName = a.Enrollment?.Student?.AppUser?.FullName ?? "",
            StudentNumber = a.Enrollment?.Student?.StudentNumber ?? "",
            SessionDate = a.SessionDate, Status = a.Status,
            Note = a.Note, ClassSessionId = a.ClassSessionId
        };
    }
}
