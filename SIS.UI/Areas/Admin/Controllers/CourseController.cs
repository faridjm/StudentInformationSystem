using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Course;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class CourseController : Controller
    {
        private readonly ICourseApi _courseApi;
        private readonly IDepartmentApi _departmentApi;
        private readonly IAcademicProgramApi _programApi;
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            ICourseApi courseApi,
            IDepartmentApi departmentApi,
            IAcademicProgramApi programApi,
            ILogger<CourseController> logger)
        {
            _courseApi = courseApi;
            _departmentApi = departmentApi;
            _programApi = programApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var request = new PagedRequest { Page = page, PageSize = 10, SearchTerm = search };
            var result = await _courseApi.GetAllAsync(request);
            ViewBag.SearchTerm = search;
            return View(result);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(dto);
            }
            try
            {
                await _courseApi.CreateAsync(dto);
                TempData["Success"] = "Kurs uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "Course Create error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                ModelState.AddModelError("", ExtractMessage(apiEx) ?? apiEx.Message);
                await LoadDropdownsAsync();
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseApi.GetByIdAsync(id);
            if (course == null) return NotFound();

            await LoadDropdownsAsync();

            var dto = new UpdateCourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Credits = course.Credits,
                Description = course.Description,
                DepartmentId = course.DepartmentId,
                AcademicProgramId = course.AcademicProgramId ?? 0
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(dto);
            }
            try
            {
                await _courseApi.UpdateAsync(dto);
                TempData["Success"] = "Kurs uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "Course Edit error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                ModelState.AddModelError("", ExtractMessage(apiEx) ?? apiEx.Message);
                await LoadDropdownsAsync();
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _courseApi.DeleteAsync(id);
                TempData["Success"] = "Kurs uğurla silindi";
            }
            catch { TempData["Error"] = "Silmə zamanı xəta baş verdi"; }
            return RedirectToAction("Index");
        }

        private async Task LoadDropdownsAsync()
        {
            var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            ViewBag.Departments = departments.Items;

            var programs = await _programApi.GetAllAsync();
            ViewBag.Programs = programs;
        }

        private static string? ExtractMessage(Refit.ApiException apiEx)
        {
            if (string.IsNullOrEmpty(apiEx.Content)) return null;
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(apiEx.Content);
                var root = json.RootElement;

                if (root.TryGetProperty("message", out var msg))
                {
                    var m = msg.GetString();
                    if (!string.IsNullOrWhiteSpace(m)) return m;
                }

                if (root.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var item in field.Value.EnumerateArray())
                            {
                                var s = item.GetString();
                                if (!string.IsNullOrWhiteSpace(s)) messages.Add(s);
                            }
                        }
                    }
                    if (messages.Count > 0) return string.Join(" • ", messages);
                }

                if (root.TryGetProperty("title", out var title))
                    return title.GetString();
            }
            catch { }
            return null;
        }
    }
}
