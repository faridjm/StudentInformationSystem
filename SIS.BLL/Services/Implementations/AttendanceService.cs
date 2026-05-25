using AutoMapper;
using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                PresentCount = list.Count(r => r.Status == SIS.Entities.Enums.AttendanceStatus.Present),
                AbsentCount = list.Count(r => r.Status == SIS.Entities.Enums.AttendanceStatus.Absent),
                LateCount = list.Count(r => r.Status == SIS.Entities.Enums.AttendanceStatus.Late),
                AttendancePercentage = await _unitOfWork.AttendanceRecords.GetAttendancePercentageAsync(enrollmentId)
            };
        }

        public async Task RecordAttendanceAsync(CreateAttendanceDto dto)
        {
            var record = _mapper.Map<AttendanceRecord>(dto);
            await _unitOfWork.AttendanceRecords.AddAsync(record);
            await _unitOfWork.SaveAsync();
        }

        public async Task RecordBulkAttendanceAsync(IEnumerable<CreateAttendanceDto> dtos)
        {
            foreach (var dto in dtos)
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
    }
}
