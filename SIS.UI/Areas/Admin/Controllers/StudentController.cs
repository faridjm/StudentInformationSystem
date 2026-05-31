using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.DTOs.Student;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class StudentController : Controller
    {
        private readonly IStudentApi _studentApi;
        private readonly IAuthApi _authApi;

        public StudentController(IStudentApi studentApi, IAuthApi authApi)
        {
            _studentApi = studentApi;
            _authApi = authApi;
        }

        public async Task<IActionResult> Index()
            => View(await _studentApi.GetAllAsync());

        public async Task<IActionResult> Detail(int id)
        {
            var s = await _studentApi.GetByIdAsync(id);
            return View(s);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _studentApi.CreateAsync(dto);
                TempData["Success"] = "Tələbə yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = await ExtractApiError(apiEx);
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var s = await _studentApi.GetByIdAsync(id);
            return View(new UpdateStudentDto { Id = s.Id, FullName = s.FullName, FinCode = s.FinCode, PhoneNumber = s.PhoneNumber });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateStudentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _studentApi.UpdateAsync(dto);
            TempData["Success"] = "Dəyişikliklər saxlandı";
            return RedirectToAction("Detail", new { id = dto.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _studentApi.DeleteAsync(id);
            TempData["Success"] = "Tələbə silindi";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetPassword(string userId, string newPassword, string confirmPassword, int studentId)
        {
            if (newPassword != confirmPassword) { TempData["Error"] = "Şifrələr uyğun deyil"; return RedirectToAction("Detail", new { id = studentId }); }
            try { await _authApi.AdminSetPasswordAsync(new AdminSetPasswordDto { UserId = userId, NewPassword = newPassword, ConfirmPassword = confirmPassword }); TempData["Success"] = "Şifrə dəyişdirildi"; }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Detail", new { id = studentId });
        }

        private static async Task<string> ExtractApiError(Refit.ApiException apiEx)
        {
            try
            {
                var content = await apiEx.GetContentAsAsync<System.Text.Json.JsonElement>();
                if (content.TryGetProperty("message", out var msg)) return msg.GetString() ?? apiEx.ReasonPhrase ?? "Xəta";
                if (content.TryGetProperty("errors", out var errs))
                    return string.Join("; ", errs.EnumerateArray().Select(e => e.GetProperty("error").GetString()));
            }
            catch { }
            return apiEx.ReasonPhrase ?? "Xəta baş verdi";
        }
    }
}
