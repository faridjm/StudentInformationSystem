using SIS.BLL.Services.DTOs.Room;

namespace SIS.BLL.Services.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetRoomsAsync();
        Task<IEnumerable<RoomDto>> GetActiveRoomsAsync();
        Task<RoomDto?> GetRoomByIdAsync(int id);

        Task<RoomDto> CreateRoomAsync(CreateRoomDto dto);
        Task UpdateRoomAsync(UpdateRoomDto dto);
        Task SetActiveAsync(int id, bool isActive);

        /// <summary>Otağın həftəlik cədvəli (təyin olunmuş aktiv qruplar).</summary>
        Task<RoomScheduleDto> GetRoomScheduleAsync(int roomId);

        /// <summary>
        /// Otağın seçilmiş slotlar üçün boş olub-olmadığını yoxlayır.
        /// Time overlap: ExistingStart &lt; NewEnd AND NewStart &lt; ExistingEnd.
        /// </summary>
        Task<RoomConflictResultDto> CheckRoomConflictAsync(
            int roomId, List<ScheduleSlotInput> slots,
            DateTime groupStart, DateTime groupEnd, int? excludeGroupId = null);

        /// <summary>Verilmiş cədvəl üçün uyğun (boş) otaqları qaytarır.</summary>
        Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(
            List<ScheduleSlotInput> slots, DateTime groupStart, DateTime groupEnd,
            int? capacityRequired = null, int? excludeGroupId = null);
    }
}
