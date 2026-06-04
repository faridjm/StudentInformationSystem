using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.DTOs.Teacher;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TeacherController : Controller
    {
        private readonly ITeacherApi _teacherApi;
        private readonly IAuthApi _authApi;
        private readonly IInstitutionalEmailApi _emailApi;

        public TeacherController(ITeacherApi teacherApi, IAuthApi authApi,
            IInstitutionalEmailApi emailApi)
        {
            _teacherApi = teacherApi;
            _authApi    = authApi;
            _emailApi   = emailApi;
        }

        public async Task<IActionResult> Index()
            => View(await _teacherApi.GetAllAsync());

        public async Task<IActionResult> Detail(int id)
            => View(await _teacherApi.GetByIdAsync(id));

        public IActionResult Create() => View();

        /// <summary>
        /// AJAX: /Admin/Teacher/GenerateEmail?firstName=Anar&amp;lastName=Əliyev
        /// → { "email": "anare1@sis.az" }
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateEmail(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return BadRequest(new { error = "Ad və soyad tələb olunur" });
            try
            {
                var result = await _emailApi.GenerateAsync(firstName, lastName);
                return Json(new { email = result.Email });
            }
            catch
            {
                return StatusCode(500, new { error = "Email generate edilmədi" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTeacherDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _teacherApi.CreateAsync(dto);
                TempData["Success"] = "Müəllim yaradıldı";
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
            var t = await _teacherApi.GetByIdAsync(id);
            return View(new UpdateTeacherDto { Id = t.Id, FullName = t.FullName, FinCode = t.FinCode, PhoneNumber = t.PhoneNumber, BirthDate = t.BirthDate });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateTeacherDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _teacherApi.UpdateAsync(dto);
            TempData["Success"] = "Dəyişikliklər saxlandı";
            return RedirectToAction("Detail", new { id = dto.Id });
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

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _teacherApi.DeleteAsync(id);
            TempData["Success"] = "Müəllim silindi";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetPassword(string userId, string newPassword, string confirmPassword, int teacherId)
        {
            if (newPassword != confirmPassword) { TempData["Error"] = "Şifrələr uyğun deyil"; return RedirectToAction("Detail", new { id = teacherId }); }
            try { await _authApi.AdminSetPasswordAsync(new AdminSetPasswordDto { UserId = userId, NewPassword = newPassword, ConfirmPassword = confirmPassword }); TempData["Success"] = "Şifrə dəyişdirildi"; }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Detail", new { id = teacherId });
        }
    }
}
