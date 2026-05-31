using Refit;
using SIS.BLL.Services.DTOs.ClassSession;

namespace SIS.UI.ApiClients
{
    public interface IClassSessionApi
    {
        [Get("/api/classsession/group/{groupId}")]
        Task<IEnumerable<ClassSessionDto>> GetByGroupAsync(int groupId);

        [Get("/api/classsession/{id}")]
        Task<ClassSessionDto> GetByIdAsync(int id);

        [Put("/api/classsession/{id}/topic")]
        Task UpdateTopicAsync(int id, [Body] UpdateTopicDto dto);

        [Put("/api/classsession/{id}/complete")]
        Task CompleteSessionAsync(int id);
    }
}
