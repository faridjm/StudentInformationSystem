using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Student;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class StudentController : Controller
    {
        private readonly IStudentApi _studentApi;
        private readonly IAcademicProgramApi _programApi;

        public StudentController(IStudentApi studentApi, IAcademicProgramApi programApi)
        {
            _studentApi = studentApi;
            _programApi = programApi;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var request = new PagedRequest { Page = page, PageSize = 10, SearchTerm = search };
            var result = await _studentApi.GetAllAsync(request);

            ViewBag.SearchTerm = search;
            return View(result);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var student = await _studentApi.GetByIdAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        public async Task<IActionResult> Create()
        {
            var programs = await _programApi.GetAllAsync();
            ViewBag.Programs = programs;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var programs = await _programApi.GetAllAsync();
                ViewBag.Programs = programs;
                return View(dto);
            }

            try
            {
                await _studentApi.CreateAsync(dto);
                TempData["Success"] = "Tələbə uğurla yaradıldı";
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
                var programs = await _programApi.GetAllAsync();
                ViewBag.Programs = programs;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentApi.GetByIdAsync(id);
            if (student == null) return NotFound();

            var programs = await _programApi.GetAllAsync();
            ViewBag.Programs = programs;

            var dto = new UpdateStudentDto
            {
                Id = student.Id,
                FullName = student.FullName,
                PhoneNumber = student.PhoneNumber,
                AcademicProgramId = 0
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var programs = await _programApi.GetAllAsync();
                ViewBag.Programs = programs;
                return View(dto);
            }

            try
            {
                await _studentApi.UpdateAsync(dto);
                TempData["Success"] = "Tələbə uğurla yeniləndi";
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
                var programs = await _programApi.GetAllAsync();
                ViewBag.Programs = programs;
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _studentApi.DeleteAsync(id);
                TempData["Success"] = "Tələbə uğurla silindi";
            }
            catch
            {
                TempData["Error"] = "Silmə zamanı xəta baş verdi";
            }
            return RedirectToAction("Index");
        }
    }
}
