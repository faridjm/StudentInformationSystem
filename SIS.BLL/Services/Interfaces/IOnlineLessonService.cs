using SIS.BLL.Services.DTOs.OnlineLesson;

namespace SIS.BLL.Services.Interfaces
{
    public interface IOnlineLessonService
    {
        // ── Lifecycle (müəllim) ──
        Task<int> CreateOnlineLessonAsync(CreateOnlineLessonDto dto, int teacherId);
        Task UpdateOnlineLessonAsync(UpdateOnlineLessonDto dto, int teacherId);
        Task<StartOnlineLessonResultDto> StartOnlineLessonAsync(int classSessionId, int teacherId);
        Task EndOnlineLessonAsync(int onlineLessonId, int teacherId);
        Task CancelOnlineLessonAsync(int onlineLessonId, int teacherId);

        // ── Tələbə ──
        Task<JoinOnlineLessonResultDto> JoinOnlineLessonAsync(int onlineLessonId, int studentId);

        // ── Sorğular ──
        Task<IEnumerable<OnlineLessonDto>> GetTeacherGroupOnlineLessonsAsync(int groupId, int teacherId);
        Task<IEnumerable<OnlineLessonDto>> GetStudentOnlineLessonsAsync(int studentId);
        Task<IEnumerable<OnlineLessonParticipantDto>> GetOnlineLessonParticipantsAsync(int onlineLessonId);

        // ── Davamiyyət inteqrasiyası ──
        Task<MarkPresentResultDto> MarkJoinedStudentsAsPresentAsync(int onlineLessonId, int teacherId);

        // ── Admin icmalı ──
        Task<IEnumerable<OnlineLessonDto>> GetAllOnlineLessonsAsync();

        /// <summary>Tək online dərsi DTO kimi qaytarır (access yoxlanışı edən).</summary>
        Task<OnlineLessonDto?> GetOnlineLessonAsync(int onlineLessonId, string requestingUserId);

        // ── Daxili Live Classroom ──

        /// <summary>İstifadəçi (teacher və ya enrolled student) classroom-a giriş icazəsi varmı?</summary>
        Task<bool> CanAccessClassroomAsync(int onlineLessonId, string userId);

        /// <summary>İstifadəçi bu dərsin sahib müəllimidirmi? (mute kimi idarəetmə əməliyyatları üçün)</summary>
        Task<bool> IsClassroomTeacherAsync(int onlineLessonId, string userId);

        Task<IEnumerable<OnlineLessonChatMessageDto>> GetClassroomChatHistoryAsync(int onlineLessonId, string requestingUserId);

        Task<OnlineLessonChatMessageDto> SaveClassroomChatMessageAsync(int onlineLessonId, string senderUserId, string text);
    }
}
