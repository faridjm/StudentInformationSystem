using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly AppDbContext _context;
        public ArchiveRepository(AppDbContext context) { _context = context; }

        public async Task<(IEnumerable<ChatConversation> Items, int Total)> GetChatConversationsAsync(
            string? search, string? role, string? status,
            DateTime? fromDate, DateTime? toDate, int? groupId,
            int page, int pageSize)
        {
            var q = _context.ChatConversations
                .Include(c => c.TeacherUser)
                .Include(c => c.StudentUser)
                .Include(c => c.Group).ThenInclude(g => g.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(c =>
                    c.TeacherUser.FullName.Contains(s)
                 || c.StudentUser.FullName.Contains(s)
                 || (c.TeacherUser.Email != null && c.TeacherUser.Email.Contains(s))
                 || (c.StudentUser.Email != null && c.StudentUser.Email.Contains(s))
                 || c.Group.Name.Contains(s));
            }

            // Role filtri — axtarışı müəllim/tələbə participant-ı üzrə daraldır
            if (string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(search))
            {
                var s = search!.Trim();
                q = q.Where(c => c.TeacherUser.FullName.Contains(s)
                              || (c.TeacherUser.Email != null && c.TeacherUser.Email.Contains(s)));
            }
            else if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(search))
            {
                var s = search!.Trim();
                q = q.Where(c => c.StudentUser.FullName.Contains(s)
                              || (c.StudentUser.Email != null && c.StudentUser.Email.Contains(s)));
            }

            if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                q = q.Where(c => !c.IsArchived);
            else if (string.Equals(status, "Archived", StringComparison.OrdinalIgnoreCase))
                q = q.Where(c => c.IsArchived);

            if (groupId.HasValue)
                q = q.Where(c => c.GroupId == groupId.Value);

            // Tarix filtri — son mesaj tarixinə görə
            if (fromDate.HasValue)
            {
                var from = fromDate.Value;
                q = q.Where(c => c.Messages.Any() && c.Messages.Max(m => m.SentAt) >= from);
            }
            if (toDate.HasValue)
            {
                var to = toDate.Value;
                q = q.Where(c => c.Messages.Any() && c.Messages.Max(m => m.SentAt) <= to);
            }

            var total = await q.CountAsync();
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;

            var items = await q
                .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<ChatConversation?> GetChatConversationDetailAsync(int conversationId)
            => await _context.ChatConversations
                .Include(c => c.TeacherUser)
                .Include(c => c.StudentUser)
                .Include(c => c.Group).ThenInclude(g => g.Course)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId)
            => await _context.ChatMessages
                .Include(m => m.SenderUser)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

        public async Task<int> GetMessageCountAsync(int conversationId)
            => await _context.ChatMessages.CountAsync(m => m.ConversationId == conversationId);

        public async Task<Dictionary<int, ChatMessage>> GetLastMessagesAsync(IEnumerable<int> conversationIds)
        {
            var ids = conversationIds.ToList();
            if (ids.Count == 0) return new Dictionary<int, ChatMessage>();

            // Hər conversation üçün ən son mesaj
            var grouped = await _context.ChatMessages
                .Where(m => ids.Contains(m.ConversationId))
                .GroupBy(m => m.ConversationId)
                .Select(g => g.OrderByDescending(m => m.SentAt).FirstOrDefault())
                .ToListAsync();

            return grouped
                .Where(m => m != null)
                .ToDictionary(m => m!.ConversationId, m => m!);
        }

        public async Task<Dictionary<int, int>> GetMessageCountsAsync(IEnumerable<int> conversationIds)
        {
            var ids = conversationIds.ToList();
            if (ids.Count == 0) return new Dictionary<int, int>();

            var counts = await _context.ChatMessages
                .Where(m => ids.Contains(m.ConversationId))
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(x => x.ConversationId, x => x.Count);
        }
    }
}
