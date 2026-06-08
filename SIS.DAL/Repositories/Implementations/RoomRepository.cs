using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(AppDbContext context) : base(context) { }

        public Task<Room?> GetByNameAsync(string name)
            => _dbSet.FirstOrDefaultAsync(r => r.Name == name);

        public Task<bool> NameExistsAsync(string name, int? excludeId = null)
            => _dbSet.AnyAsync(r => r.Name == name && (!excludeId.HasValue || r.Id != excludeId.Value));

        public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
            => await _dbSet.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();

        public async Task<List<Group>> GetRoomGroupsWithScheduleAsync(
            int roomId, int? excludeGroupId = null,
            DateTime? rangeStart = null, DateTime? rangeEnd = null)
        {
            var query = _context.Set<Group>()
                .Include(g => g.Course)
                .Include(g => g.Teacher).ThenInclude(t => t.AppUser)
                .Include(g => g.ScheduleDays)
                .Where(g => g.RoomId == roomId && g.IsActive)
                .AsNoTracking()
                .AsQueryable();

            if (excludeGroupId.HasValue)
                query = query.Where(g => g.Id != excludeGroupId.Value);

            // Tarix aralığı üst-üstə düşməsi: g.Start <= rangeEnd && rangeStart <= g.End
            if (rangeStart.HasValue && rangeEnd.HasValue)
                query = query.Where(g => g.StartDate <= rangeEnd.Value && rangeStart.Value <= g.EndDate);

            return await query.ToListAsync();
        }
    }
}
