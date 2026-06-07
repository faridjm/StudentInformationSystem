using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Refit;
using SIS.BLL.Services.DTOs.SystemUsers;
using SIS.Common.Constants;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    /// <summary>
    /// SuperAdmin-only — sistem istifadəçiləri (Admin/Moderator/Teacher/Student/SuperAdmin) idarəetməsi.
    /// </summary>
    [Area("Admin")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = RoleConstants.SuperAdmin)]
    public class SystemUsersController : Controller
    {
        private readonly ISystemUsersApi _api;
        public SystemUsersController(ISystemUsersApi api) { _api = api; }

        public async Task<IActionResult> Index(
            string? search, string? role, string? status, int page = 1, int pageSize = 20)
        {
            try
            {
                var res = await _api.GetPagedAsync(search, role, status, page, pageSize);
                ViewBag.Search = search;
                ViewBag.Role = role;
                ViewBag.Status = status ?? "all";
                ViewBag.PageSize = pageSize;
                return View(res);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ExtractError(ex, "Sistemlər siyahısı yüklənmədi");
                return View(new SIS.BLL.Services.DTOs.Common.PagedResult<SystemUserListItemDto>());
            }
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateSystemUserDto { IsActive = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSystemUserDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _api.CreateAsync(dto);
                TempData["Success"] = "İstifadəçi yaradıldı";
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ExtractError(ex, "İstifadəçi yaradıla bilmədi"));
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var d = await _api.GetByIdAsync(id);
                var dto = new UpdateSystemUserDto
                {
                    FullName = d.FullName,
                    Email = d.Email ?? "",
                    PhoneNumber = d.PhoneNumber,
                    Role = d.Role,
                    IsActive = d.IsActive,
                    EmailConfirmed = d.EmailConfirmed
                };
                ViewBag.UserId = d.Id;
                ViewBag.PanelAccess = d.PanelAccess;
                ViewBag.AdminAccessActive = d.AdminAccessActive;
                return View(dto);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ExtractError(ex, "İstifadəçi tapılmadı");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateSystemUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                return View(dto);
            }
            try
            {
                await _api.UpdateAsync(id, dto);
                TempData["Success"] = "İstifadəçi yeniləndi";
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ExtractError(ex, "Yeniləmə alınmadı"));
                ViewBag.UserId = id;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var d = await _api.GetByIdAsync(id);
                return View(d);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ExtractError(ex, "İstifadəçi tapılmadı");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            try
            {
                var d = await _api.GetByIdAsync(id);
                ViewBag.UserId = d.Id;
                ViewBag.UserFullName = d.FullName;
                ViewBag.UserEmail = d.Email;
                return View(new ChangeSystemUserPasswordDto());
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ExtractError(ex, "İstifadəçi tapılmadı");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, ChangeSystemUserPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword != dto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Şifrələr uyğun gəlmir və ya boşdur");
                ViewBag.UserId = id;
                return View(dto);
            }
            try
            {
                await _api.ChangePasswordAsync(id, dto);
                TempData["Success"] = "Şifrə dəyişdirildi";
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ExtractError(ex, "Şifrə dəyişdirilə bilmədi"));
                ViewBag.UserId = id;
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(string id)
        {
            try { await _api.EnableAsync(id); TempData["Success"] = "İstifadəçi aktivləşdirildi"; }
            catch (ApiException ex) { TempData["Error"] = ExtractError(ex, "Əməliyyat alınmadı"); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(string id)
        {
            try { await _api.DisableAsync(id); TempData["Success"] = "İstifadəçi deaktiv edildi"; }
            catch (ApiException ex) { TempData["Error"] = ExtractError(ex, "Əməliyyat alınmadı"); }
            return RedirectToAction(nameof(Index));
        }

        private static string ExtractError(ApiException ex, string fallback)
        {
            try
            {
                if (string.IsNullOrEmpty(ex.Content)) return fallback;
                var doc = System.Text.Json.JsonDocument.Parse(ex.Content);
                if (doc.RootElement.TryGetProperty("message", out var m)) return m.GetString() ?? fallback;
                if (doc.RootElement.TryGetProperty("Message", out var m2)) return m2.GetString() ?? fallback;
            }
            catch { }
            return fallback;
        }
    }
}
