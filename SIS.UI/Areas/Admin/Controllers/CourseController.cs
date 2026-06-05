using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Course;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class CourseController : Controller
    {
        private readonly ICourseApi _courseApi;
        private readonly IDepartmentApi _departmentApi;
        public CourseController(ICourseApi courseApi, IDepartmentApi departmentApi)
        {
            _courseApi = courseApi;
            _departmentApi = departmentApi;
        }

        public async Task<IActionResult> Index()
            => View(await _courseApi.GetAllAsync());

        // Department dropdown üçün aktiv şöbələr. API əlçatan deyilsə boş siyahı.
        private async Task LoadDepartmentsAsync()
        {
            try { ViewBag.Departments = (await _departmentApi.GetAllAsync()).Where(d => d.IsActive).ToList(); }
            catch { ViewBag.Departments = new List<SIS.BLL.Services.DTOs.Department.DepartmentDto>(); }
        }

        public async Task<IActionResult> Create()
        {
            await LoadDepartmentsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseDto dto)
        {
            if (!ModelState.IsValid) { await LoadDepartmentsAsync(); return View(dto); }
            await _courseApi.CreateAsync(dto);
            TempData["Success"] = "Kurs yaradıldı";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseApi.GetByIdAsync(id);
            await LoadDepartmentsAsync();
            return View(new UpdateCourseDto { Id = course.Id, Name = course.Name, Description = course.Description, IsActive = course.IsActive,
                Credit = course.Credit, TotalHours = course.TotalHours, Level = course.Level, DepartmentId = course.DepartmentId });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCourseDto dto)
        {
            if (!ModelState.IsValid) { await LoadDepartmentsAsync(); return View(dto); }
            await _courseApi.UpdateAsync(dto);
            TempData["Success"] = "Dəyişikliklər saxlandı";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseApi.DeleteAsync(id);
            TempData["Success"] = "Kurs silindi";
            return RedirectToAction("Index");
        }
    }
}
