using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IOnlineLessonRepository : IGenericRepository<OnlineLesson>
    {
        /// <summary>Online dərsi bütün əlaqələri ilə (Group, ClassSession, Teacher, Participants) gətirir.</summary>
        Task<OnlineLesson?> GetWithDetailsAsync(int id);

        /// <summary>Verilmiş sessiya üçün statusu Scheduled və ya Live olan aktiv dərsi qaytarır.</summary>
        Task<OnlineLesson?> GetActiveBySessionAsync(int classSessionId);

        Task<IEnumerable<OnlineLesson>> GetByGroupAsync(int groupId);
        Task<IEnumerable<OnlineLesson>> GetByTeacherAsync(int teacherId);

        /// <summary>Tələbənin aid olduğu qrupların online dərslərini qaytarır.</summary>
        Task<IEnumerable<OnlineLesson>> GetByStudentAsync(int studentId);

        /// <summary>Admin icmalı üçün bütün dərsləri əlaqələri ilə gətirir.</summary>
        Task<IEnumerable<OnlineLesson>> GetAllWithDetailsAsync();

        Task<OnlineLessonParticipant?> GetParticipantAsync(int onlineLessonId, int studentId);
        Task<IEnumerable<OnlineLessonParticipant>> GetParticipantsAsync(int onlineLessonId);

        Task AddParticipantAsync(OnlineLessonParticipant participant);
        void UpdateParticipant(OnlineLessonParticipant participant);

        // ── Online classroom chat ──
        Task AddChatMessageAsync(OnlineLessonChatMessage message);
        Task<IEnumerable<OnlineLessonChatMessage>> GetChatMessagesAsync(int onlineLessonId, int take = 200);
    }
}
