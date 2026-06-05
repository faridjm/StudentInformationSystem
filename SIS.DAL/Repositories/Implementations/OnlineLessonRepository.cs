using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class OnlineLessonRepository : GenericRepository<OnlineLesson>, IOnlineLessonRepository
    {
        private readonly DbSet<OnlineLessonParticipant> _participants;
        private readonly DbSet<OnlineLessonChatMessage> _chatMessages;

        public OnlineLessonRepository(AppDbContext context) : base(context)
        {
            _participants = context.Set<OnlineLessonParticipant>();
            _chatMessages = context.Set<OnlineLessonChatMessage>();
        }

        public async Task<OnlineLesson?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(o => o.Group).ThenInclude(g => g.Course)
                .Include(o => o.ClassSession)
                .Include(o => o.Teacher).ThenInclude(t => t.AppUser)
                .Include(o => o.Participants).ThenInclude(p => p.Student).ThenInclude(s => s.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<OnlineLesson?> GetActiveBySessionAsync(int classSessionId)
            => await _dbSet
                .Where(o => o.ClassSessionId == classSessionId
                    && (o.Status == OnlineLessonStatus.Scheduled || o.Status == OnlineLessonStatus.Live))
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

        public async Task<IEnumerable<OnlineLesson>> GetByGroupAsync(int groupId)
            => await _dbSet
                .Include(o => o.ClassSession)
                .Include(o => o.Participants)
                .Where(o => o.GroupId == groupId)
                .OrderByDescending(o => o.ClassSession.Date)
                .ToListAsync();

        public async Task<IEnumerable<OnlineLesson>> GetByTeacherAsync(int teacherId)
            => await _dbSet
                .Include(o => o.Group).ThenInclude(g => g.Course)
                .Include(o => o.ClassSession)
                .Include(o => o.Participants)
                .Where(o => o.TeacherId == teacherId)
                .OrderByDescending(o => o.ClassSession.Date)
                .ToListAsync();

        public async Task<IEnumerable<OnlineLesson>> GetByStudentAsync(int studentId)
        {
            // Tələbənin aktiv qeydiyyatda olduğu qrupların id-ləri
            var groupIds = await _context.Set<Enrollment>()
                .Where(e => e.StudentId == studentId)
                .Select(e => e.GroupId)
                .Distinct()
                .ToListAsync();

            return await _dbSet
                .Include(o => o.Group).ThenInclude(g => g.Course)
                .Include(o => o.ClassSession)
                .Include(o => o.Participants)
                .Where(o => groupIds.Contains(o.GroupId))
                .OrderByDescending(o => o.ClassSession.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineLesson>> GetAllWithDetailsAsync()
            => await _dbSet
                .Include(o => o.Group).ThenInclude(g => g.Course)
                .Include(o => o.Teacher).ThenInclude(t => t.AppUser)
                .Include(o => o.ClassSession)
                .Include(o => o.Participants)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

        public async Task<OnlineLessonParticipant?> GetParticipantAsync(int onlineLessonId, int studentId)
            => await _participants
                .FirstOrDefaultAsync(p => p.OnlineLessonId == onlineLessonId && p.StudentId == studentId);

        public async Task<IEnumerable<OnlineLessonParticipant>> GetParticipantsAsync(int onlineLessonId)
            => await _participants
                .Include(p => p.Student).ThenInclude(s => s.AppUser)
                .Where(p => p.OnlineLessonId == onlineLessonId)
                .ToListAsync();

        public async Task AddParticipantAsync(OnlineLessonParticipant participant)
            => await _participants.AddAsync(participant);

        public void UpdateParticipant(OnlineLessonParticipant participant)
        {
            var entry = _context.Entry(participant);
            if (entry.State == EntityState.Detached)
                _participants.Attach(participant);
            entry.State = EntityState.Modified;
            participant.UpdatedAt = DateTime.UtcNow;
        }

        // ── Online classroom chat ──

        public async Task AddChatMessageAsync(OnlineLessonChatMessage message)
            => await _chatMessages.AddAsync(message);

        public async Task<IEnumerable<OnlineLessonChatMessage>> GetChatMessagesAsync(int onlineLessonId, int take = 200)
            => await _chatMessages
                .Include(m => m.Sender)
                .Where(m => m.OnlineLessonId == onlineLessonId)
                .OrderBy(m => m.SentAt)
                .Take(take)
                .ToListAsync();
    }
}