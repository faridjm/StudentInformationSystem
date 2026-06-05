using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Department;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentApi _departmentApi;
        public DepartmentController(IDepartmentApi departmentApi) { _departmentApi = departmentApi; }

        public async Task<IActionResult> Index()
            => View(await _departmentApi.GetAllAsync());

        public async Task<IActionResult> Details(int id)
        {
            var d = await _departmentApi.GetByIdAsync(id);
            return View(d);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _departmentApi.CreateAsync(dto);
                TempData["Success"] = "Şöbə yaradıldı";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Şöbə yaradıla bilmədi";
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var d = await _departmentApi.GetByIdAsync(id);
            return View(new UpdateDepartmentDto { Id = d.Id, Name = d.Name, Description = d.Description, IsActive = d.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateDepartmentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _departmentApi.UpdateAsync(dto);
            TempData["Success"] = "Dəyişikliklər saxlandı";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id, bool isActive)
        {
            await _departmentApi.SetActiveAsync(id, isActive);
            TempData["Success"] = isActive ? "Şöbə aktiv edildi" : "Şöbə deaktiv edildi";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _departmentApi.DeleteAsync(id);
            TempData["Success"] = "Şöbə silindi";
            return RedirectToAction("Index");
        }
    }
}
