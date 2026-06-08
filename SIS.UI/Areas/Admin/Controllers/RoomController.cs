using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Room;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class RoomController : Controller
    {
        private readonly IRoomApi _roomApi;

        public RoomController(IRoomApi roomApi)
        {
            _roomApi = roomApi;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomApi.GetAllAsync();
            return View(rooms);
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateRoomDto());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _roomApi.CreateAsync(dto);
                TempData["Success"] = "Otaq yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException ex)
            {
                TempData["Error"] = string.IsNullOrEmpty(ex.Content) ? ex.Message : ex.Content;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var r = await _roomApi.GetByIdAsync(id);
                var dto = new UpdateRoomDto
                {
                    Id = r.Id, Name = r.Name, Code = r.Code, Type = r.Type,
                    Capacity = r.Capacity, Location = r.Location, Floor = r.Floor,
                    Description = r.Description, IsActive = r.IsActive
                };
                return View(dto);
            }
            catch
            {
                TempData["Error"] = "Otaq tapılmadı";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateRoomDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _roomApi.UpdateAsync(dto);
                TempData["Success"] = "Otaq yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException ex)
            {
                TempData["Error"] = string.IsNullOrEmpty(ex.Content) ? ex.Message : ex.Content;
                return View(dto);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var schedule = await _roomApi.GetScheduleAsync(id);
                return View(schedule);
            }
            catch
            {
                TempData["Error"] = "Otaq tapılmadı";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool active)
        {
            try
            {
                await _roomApi.SetActiveAsync(id, active);
                TempData["Success"] = active ? "Otaq aktiv edildi" : "Otaq deaktiv edildi";
            }
            catch (Refit.ApiException ex)
            {
                TempData["Error"] = string.IsNullOrEmpty(ex.Content) ? ex.Message : ex.Content;
            }
            return RedirectToAction("Index");
        }
    }
}
