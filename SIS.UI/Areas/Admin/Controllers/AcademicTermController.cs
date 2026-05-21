using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AcademicTerm;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AcademicTermController : Controller
    {
        private readonly IAcademicTermApi _termApi;

        public AcademicTermController(IAcademicTermApi termApi)
        {
            _termApi = termApi;
        }

        public async Task<IActionResult> Index()
        {
            var terms = await _termApi.GetAllAsync();
            return View(terms);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateAcademicTermDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _termApi.CreateAsync(dto);
                TempData["Success"] = "Termin uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var term = await _termApi.GetByIdAsync(id);
            if (term == null) return NotFound();
            var dto = new UpdateAcademicTermDto
            {
                Id = term.Id,
                Name = term.Name,
                StartDate = term.StartDate,
                EndDate = term.EndDate
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateAcademicTermDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _termApi.UpdateAsync(dto);
                TempData["Success"] = "Termin uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(int id)
        {
            try
            {
                await _termApi.SetActiveAsync(id);
                TempData["Success"] = "Termin aktiv edildi";
            }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _termApi.DeleteAsync(id);
                TempData["Success"] = "Termin uğurla silindi";
            }
            catch { TempData["Error"] = "Silmə zamanı xəta baş verdi"; }
            return RedirectToAction("Index");
        }
    }
}
