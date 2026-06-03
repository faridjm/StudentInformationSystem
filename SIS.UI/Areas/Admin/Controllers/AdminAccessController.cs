using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AdminAccess;
using SIS.Common.Constants;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public class AdminAccessController : Controller
    {
        private readonly IAdminAccessApi _api;
        public AdminAccessController(IAdminAccessApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            var users = await _api.GetUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(string adminUserId, DateTime? expiresAt, string? reason)
        {
            try
            {
                await _api.ActivateAsync(new ActivateAdminAccessDto
                {
                    AdminUserId = adminUserId,
                    ExpiresAt = expiresAt,
                    Reason = reason
                });
                TempData["Success"] = "Admin panel girişi aktivləşdirildi";
            }
            catch
            {
                TempData["Error"] = "Əməliyyat alınmadı (öz access-inizi aktiv edə bilməzsiniz və ya istifadəçi Admin deyil)";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(string adminUserId, string? reason)
        {
            try
            {
                await _api.DeactivateAsync(new DeactivateAdminAccessDto
                {
                    AdminUserId = adminUserId,
                    Reason = reason
                });
                TempData["Success"] = "Admin panel girişi dayandırıldı";
            }
            catch
            {
                TempData["Error"] = "Əməliyyat alınmadı (aktiv giriş yoxdur)";
            }
            return RedirectToAction("Index");
        }
    }
}
