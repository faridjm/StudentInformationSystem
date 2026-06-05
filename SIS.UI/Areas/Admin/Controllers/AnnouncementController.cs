using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AnnouncementController : Controller
    {
        private readonly IAnnouncementApi _announcementApi;
        private readonly IGroupApi _groupApi;

        public AnnouncementController(IAnnouncementApi announcementApi, IGroupApi groupApi)
        {
            _announcementApi = announcementApi;
            _groupApi = groupApi;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementApi.GetAllAsync();
            return View(announcements);
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var d = await _announcementApi.GetDetailAsync(id);
                return View(d);
            }
            catch
            {
                TempData["Error"] = "Elan tapılmadı";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Create()
        {
            await LoadGroupsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAnnouncementDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadGroupsAsync();
                return View(dto);
            }
            try
            {
                await _announcementApi.CreateAsync(dto);
                TempData["Success"] = "Elan uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = await ExtractApiError(apiEx);
                await LoadGroupsAsync();
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                await LoadGroupsAsync();
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _announcementApi.DeleteAsync(id);
                TempData["Success"] = "Elan uğurla silindi";
            }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Index");
        }

        /// <summary>AJAX: Selected Users üçün axtarış.</summary>
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Json(Array.Empty<UserSearchResultDto>());
            try
            {
                var results = await _announcementApi.SearchUsersAsync(query);
                return Json(results);
            }
            catch
            {
                return Json(Array.Empty<UserSearchResultDto>());
            }
        }

        // ── Helpers ──
        private async Task LoadGroupsAsync()
        {
            try { ViewBag.Groups = (await _groupApi.GetAllAsync()).ToList(); }
            catch { ViewBag.Groups = new List<SIS.BLL.Services.DTOs.Groups.GroupListDto>(); }
        }

        private static async Task<string> ExtractApiError(Refit.ApiException apiEx)
        {
            try
            {
                var content = await apiEx.GetContentAsAsync<System.Text.Json.JsonElement>();
                if (content.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? apiEx.ReasonPhrase ?? "Xəta";
            }
            catch { }
            return apiEx.ReasonPhrase ?? "Xəta baş verdi";
        }
    }
}
