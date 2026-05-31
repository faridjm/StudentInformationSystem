using SIS.BLL.Services.DTOs.ClassSession;

namespace SIS.BLL.Services.Interfaces
{
    public interface IClassSessionService
    {
        Task<IEnumerable<ClassSessionDto>> GetByGroupAsync(int groupId);
        Task<ClassSessionDto?> GetByIdAsync(int id);
        Task UpdateTopicAsync(int sessionId, string topic);
        Task CompleteSessionAsync(int sessionId);
    }
}
