using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context) { _context = context; }

        public async Task<ChatConversation?> GetConversationAsync(int conversationId)
            => await _context.ChatConversations
                .Include(c => c.TeacherUser)
                .Include(c => c.StudentUser)
                .Include(c => c.Group).ThenInclude(g => g.Course)
                .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted);

        public async Task<ChatConversation?> FindConversationAsync(int groupId, string teacherUserId, string studentUserId)
            => await _context.ChatConversations
                .Include(c => c.TeacherUser)
                .Include(c => c.StudentUser)
                .Include(c => c.Group).ThenInclude(g => g.Course)
                .FirstOrDefaultAsync(c => c.GroupId == groupId
                    && c.TeacherUserId == teacherUserId
                    && c.StudentUserId == studentUserId
                    && !c.IsDeleted);

        public async Task<IEnumerable<ChatConversation>> GetConversationsByUserAsync(string userId)
            => await _context.ChatConversations
                .Include(c => c.TeacherUser)
                .Include(c => c.StudentUser)
                .Include(c => c.Group).ThenInclude(g => g.Course)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => (c.TeacherUserId == userId || c.StudentUserId == userId) && !c.IsDeleted)
                .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt)
                .ToListAsync();

        public async Task<ChatConversation> CreateConversationAsync(ChatConversation conversation)
        {
            conversation.CreatedAt = DateTime.UtcNow;
            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            message.CreatedAt = DateTime.UtcNow;
            message.SentAt = DateTime.UtcNow;
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 50, int skip = 0)
            => await _context.ChatMessages
                .Include(m => m.SenderUser)
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip).Take(take)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

        public async Task MarkAsReadAsync(int conversationId, string userId)
        {
            var unread = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId
                    && m.SenderUserId != userId
                    && !m.IsRead && !m.IsDeleted)
                .ToListAsync();

            foreach (var m in unread)
                m.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _context.ChatMessages
                .Where(m => !m.IsDeleted && !m.IsRead
                    && m.SenderUserId != userId
                    && (m.Conversation.TeacherUserId == userId || m.Conversation.StudentUserId == userId))
                .CountAsync();

        public async Task<int> GetUnreadCountForConversationAsync(int conversationId, string userId)
            => await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId
                    && m.SenderUserId != userId
                    && !m.IsRead && !m.IsDeleted)
                .CountAsync();
    }
}
