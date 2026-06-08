using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task<Room?> GetByNameAsync(string name);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<IEnumerable<Room>> GetActiveRoomsAsync();

        /// <summary>
        /// Verilmiş otağa təyin edilmiş, silinməmiş və aktiv qrupları (cədvəl günləri,
        /// kurs, müəllim ilə birlikdə) gətirir. Conflict / schedule view üçün.
        /// Tarix aralığı verilərsə yalnız üst-üstə düşən qruplar qaytarılır.
        /// </summary>
        Task<List<Group>> GetRoomGroupsWithScheduleAsync(
            int roomId, int? excludeGroupId = null,
            DateTime? rangeStart = null, DateTime? rangeEnd = null);
    }
}
