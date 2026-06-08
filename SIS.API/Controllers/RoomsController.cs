using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Room;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    /// <summary>
    /// Akademik otaq idarəetməsi (real scheduling). Yalnız Admin / SuperAdmin.
    /// Moderator / Teacher / Student giriş edə bilməz.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _roomService.GetRoomsAsync());

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
            => Ok(await _roomService.GetActiveRoomsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            return room == null ? NotFound() : Ok(room);
        }

        [HttpGet("{id}/schedule")]
        public async Task<IActionResult> GetSchedule(int id)
        {
            try { return Ok(await _roomService.GetRoomScheduleAsync(id)); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            try { return Ok(await _roomService.CreateRoomAsync(dto)); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRoomDto dto)
        {
            try
            {
                await _roomService.UpdateRoomAsync(dto);
                return Ok(new { Message = "Otaq yeniləndi" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPut("{id}/set-active")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool active)
        {
            try
            {
                await _roomService.SetActiveAsync(id, active);
                return Ok(new { Message = active ? "Otaq aktiv edildi" : "Otaq deaktiv edildi" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        /// <summary>Group schedule seçildikdə uyğun (boş) otaqları qaytarır.</summary>
        [HttpPost("available")]
        public async Task<IActionResult> Available([FromBody] RoomQueryRequest req)
        {
            var rooms = await _roomService.GetAvailableRoomsAsync(
                req.Slots, req.GroupStart, req.GroupEnd, req.CapacityRequired, req.ExcludeGroupId);
            return Ok(rooms);
        }

        /// <summary>Konkret otaq üçün conflict yoxlaması.</summary>
        [HttpPost("check-conflict")]
        public async Task<IActionResult> CheckConflict([FromBody] RoomQueryRequest req)
        {
            if (!req.RoomId.HasValue)
                return BadRequest(new { error = "RoomId tələb olunur" });

            var result = await _roomService.CheckRoomConflictAsync(
                req.RoomId.Value, req.Slots, req.GroupStart, req.GroupEnd, req.ExcludeGroupId);
            return Ok(result);
        }
    }
}
