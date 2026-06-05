using Refit;
using SIS.BLL.Services.DTOs.OnlineLesson;

namespace SIS.UI.ApiClients
{
    public interface IOnlineLessonApi
    {
        [Post("/api/onlinelesson")]
        Task<CreateOnlineLessonResponse> CreateAsync([Body] CreateOnlineLessonDto dto);

        [Put("/api/onlinelesson/{id}")]
        Task UpdateAsync(int id, [Body] UpdateOnlineLessonDto dto);

        [Post("/api/onlinelesson/session/{sessionId}/start")]
        Task<StartOnlineLessonResultDto> StartAsync(int sessionId);

        [Post("/api/onlinelesson/{id}/end")]
        Task EndAsync(int id);

        [Post("/api/onlinelesson/{id}/cancel")]
        Task CancelAsync(int id);

        [Post("/api/onlinelesson/{id}/join")]
        Task<JoinOnlineLessonResultDto> JoinAsync(int id);

        [Get("/api/onlinelesson/group/{groupId}")]
        Task<IEnumerable<OnlineLessonDto>> GetByGroupAsync(int groupId);

        [Get("/api/onlinelesson/{id}")]
        Task<OnlineLessonDto> GetByIdAsync(int id);

        [Get("/api/onlinelesson/student")]
        Task<IEnumerable<OnlineLessonDto>> GetForStudentAsync();

        [Get("/api/onlinelesson/{id}/participants")]
        Task<IEnumerable<OnlineLessonParticipantDto>> GetParticipantsAsync(int id);

        [Post("/api/onlinelesson/{id}/mark-present")]
        Task<MarkPresentResultDto> MarkPresentAsync(int id);

        [Get("/api/onlinelesson/all")]
        Task<IEnumerable<OnlineLessonDto>> GetAllAsync();

        // ── Daxili Live Classroom Chat ──

        [Get("/api/onlinelesson/{id}/chat")]
        Task<IEnumerable<OnlineLessonChatMessageDto>> GetChatAsync(int id);

        [Post("/api/onlinelesson/{id}/chat")]
        Task<OnlineLessonChatMessageDto> SendChatAsync(int id, [Body] SendClassroomChatDto dto);
    }

    /// <summary>Create endpoint-inin { id } cavabı.</summary>
    public class CreateOnlineLessonResponse
    {
        public int Id { get; set; }
    }
}
