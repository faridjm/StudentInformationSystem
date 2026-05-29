using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Auth;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class ProfileController : Controller
    {
        private readonly IAuthApi _authApi;

        public ProfileController(IAuthApi authApi)
        {
            _authApi = authApi;
        }

        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                TempData["Error"] = "Şifrələr uyğun gəlmir";
                return View();
            }
            try
            {
                await _authApi.SetPasswordAsync(dto);
                TempData["Success"] = "Şifrə uğurla dəyişdirildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return View();
        }
    }
}
