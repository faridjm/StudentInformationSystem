using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.DTOs.Teacher;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TeacherController : Controller
    {
        private readonly ITeacherApi _teacherApi;
        private readonly IDepartmentApi _departmentApi;
        private readonly IAuthApi _authApi;

        public TeacherController(ITeacherApi teacherApi, IDepartmentApi departmentApi, IAuthApi authApi)
        {
            _teacherApi = teacherApi;
            _departmentApi = departmentApi;
            _authApi = authApi;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var request = new PagedRequest { Page = page, PageSize = 10, SearchTerm = search };
            var result = await _teacherApi.GetAllAsync(request);
            ViewBag.SearchTerm = search;
            return View(result);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var teacher = await _teacherApi.GetByIdAsync(id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        public async Task<IActionResult> Create()
        {
            var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            ViewBag.Departments = departments.Items;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }

            try
            {
                await _teacherApi.CreateAsync(dto);
                TempData["Success"] = "Müəllim uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException ex)
            {
                var message = ex.Content ?? ex.ReasonPhrase ?? "Xəta baş verdi";
                try
                {
                    var error = System.Text.Json.JsonDocument.Parse(message);
                    if (error.RootElement.TryGetProperty("message", out var msg))
                        message = msg.GetString() ?? message;
                    else if (error.RootElement.TryGetProperty("errors", out var errors))
                        message = errors.ToString();
                }
                catch { }
                ModelState.AddModelError("", message);
                var departments = await _departmentApi.GetAllAsync(
                    new SIS.Common.Pagination.PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var departments = await _departmentApi.GetAllAsync(
                    new SIS.Common.Pagination.PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var teacher = await _teacherApi.GetByIdAsync(id);
            if (teacher == null) return NotFound();

            var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            ViewBag.Departments = departments.Items;

            var dto = new UpdateTeacherDto
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                PhoneNumber = teacher.PhoneNumber,
                Title = teacher.Title,
                DepartmentId = 0
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }

            try
            {
                await _teacherApi.UpdateAsync(dto);
                TempData["Success"] = "Müəllim uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException ex)
            {
                var message = ex.Content ?? ex.ReasonPhrase ?? "Xəta baş verdi";
                try
                {
                    var error = System.Text.Json.JsonDocument.Parse(message);
                    if (error.RootElement.TryGetProperty("message", out var msg))
                        message = msg.GetString() ?? message;
                    else if (error.RootElement.TryGetProperty("errors", out var errors))
                        message = errors.ToString();
                }
                catch { }
                ModelState.AddModelError("", message);
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _teacherApi.DeleteAsync(id);
                TempData["Success"] = "Müəllim uğurla silindi";
            }
            catch
            {
                TempData["Error"] = "Silmə zamanı xəta baş verdi";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(string userId, string newPassword, string confirmPassword, int teacherId)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Şifrələr uyğun gəlmir";
                return RedirectToAction("Detail", new { id = teacherId });
            }
            try
            {
                await _authApi.AdminSetPasswordAsync(new AdminSetPasswordDto
                {
                    UserId = userId,
                    NewPassword = newPassword,
                    ConfirmPassword = confirmPassword
                });
                TempData["Success"] = "Şifrə uğurla dəyişdirildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detail", new { id = teacherId });
        }
    }
}
