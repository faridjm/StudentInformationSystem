using Refit;
using SIS.BLL.Services.DTOs.Room;

namespace SIS.UI.ApiClients
{
    public interface IRoomApi
    {
        [Get("/api/rooms")]
        Task<IEnumerable<RoomDto>> GetAllAsync();

        [Get("/api/rooms/active")]
        Task<IEnumerable<RoomDto>> GetActiveAsync();

        [Get("/api/rooms/{id}")]
        Task<RoomDto> GetByIdAsync(int id);

        [Get("/api/rooms/{id}/schedule")]
        Task<RoomScheduleDto> GetScheduleAsync(int id);

        [Post("/api/rooms")]
        Task<RoomDto> CreateAsync([Body] CreateRoomDto dto);

        [Put("/api/rooms")]
        Task UpdateAsync([Body] UpdateRoomDto dto);

        [Put("/api/rooms/{id}/set-active")]
        Task SetActiveAsync(int id, bool active);

        [Post("/api/rooms/available")]
        Task<IEnumerable<AvailableRoomDto>> AvailableAsync([Body] RoomQueryRequest req);

        [Post("/api/rooms/check-conflict")]
        Task<RoomConflictResultDto> CheckConflictAsync([Body] RoomQueryRequest req);
    }
}
