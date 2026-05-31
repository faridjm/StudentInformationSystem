using SIS.BLL.Services.DTOs.ClassSession;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class ClassSessionService : IClassSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClassSessionService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<IEnumerable<ClassSessionDto>> GetByGroupAsync(int groupId)
        {
            var sessions = await _unitOfWork.ClassSessions.GetByGroupAsync(groupId);
            return sessions.Select(MapToDto);
        }

        public async Task<ClassSessionDto?> GetByIdAsync(int id)
        {
            var s = await _unitOfWork.ClassSessions.GetByIdAsync(id);
            return s == null ? null : MapToDto(s);
        }

        public async Task UpdateTopicAsync(int sessionId, string topic)
        {
            var session = await _unitOfWork.ClassSessions.GetByIdAsync(sessionId)
                ?? throw new NotFoundException("Sessiya tapılmadı");
            session.Topic = topic;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.ClassSessions.Update(session);
            await _unitOfWork.SaveAsync();
        }

        public async Task CompleteSessionAsync(int sessionId)
        {
            var session = await _unitOfWork.ClassSessions.GetByIdAsync(sessionId)
                ?? throw new NotFoundException("Sessiya tapılmadı");
            session.Status = Entities.Enums.ClassSessionStatus.Completed;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.ClassSessions.Update(session);
            await _unitOfWork.SaveAsync();
        }

        private static ClassSessionDto MapToDto(ClassSession s) => new()
        {
            Id = s.Id, GroupId = s.GroupId, SessionNumber = s.SessionNumber,
            Date = s.Date, StartTime = s.StartTime.ToString("HH:mm"),
            EndTime = s.EndTime.ToString("HH:mm"), Room = s.Room,
            Topic = s.Topic, Notes = s.Notes, Status = s.Status,
            CancellationReason = s.CancellationReason,
            CourseName = s.Group?.Course?.Name ?? "",
            TeacherName = s.Group?.Teacher?.AppUser?.FullName ?? "",
            CourseCode = s.Group?.Course?.Name ?? ""
        };
    }
}
