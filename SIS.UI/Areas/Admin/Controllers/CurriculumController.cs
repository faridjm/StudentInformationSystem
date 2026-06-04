using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Curriculum;
using SIS.Common.Constants;
using SIS.Entities.Enums;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
    public class CurriculumController : Controller
    {
        private readonly ICurriculumTemplateApi _api;
        private readonly ICourseApi _courseApi;

        public CurriculumController(ICurriculumTemplateApi api, ICourseApi courseApi)
        {
            _api = api;
            _courseApi = courseApi;
        }

        public async Task<IActionResult> Index()
        {
            var templates = (await _api.GetAllAsync()).ToList();
            ViewBag.Courses = (await _courseApi.GetAllAsync()).ToList();
            return View(templates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, string name, string? description)
        {
            try
            {
                var t = await _api.CreateAsync(new CreateCurriculumTemplateDto { CourseId = courseId, Name = name, Description = description });
                TempData["Success"] = "Şablon yaradıldı";
                return RedirectToAction("Manage", new { id = t.Id });
            }
            catch { TempData["Error"] = "Şablon yaradıla bilmədi"; return RedirectToAction("Index"); }
        }

        public async Task<IActionResult> Manage(int id)
        {
            var template = await _api.GetDetailAsync(id);
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int templateId, int sessionNumber, string topic,
            bool hasAssessment, string? assessmentTitle, int? assessmentType, decimal? weightPercent, decimal? maxScore)
        {
            try
            {
                await _api.AddItemAsync(new CreateTemplateItemDto
                {
                    CurriculumTemplateId = templateId,
                    SessionNumber = sessionNumber,
                    Topic = topic,
                    HasAssessment = hasAssessment,
                    AssessmentTitle = hasAssessment ? assessmentTitle : null,
                    AssessmentType = hasAssessment && assessmentType.HasValue ? (AssessmentType)assessmentType.Value : null,
                    WeightPercent = hasAssessment ? weightPercent : null,
                    MaxScore = hasAssessment ? maxScore : null
                });
                TempData["Success"] = "Sıra əlavə edildi";
            }
            catch { TempData["Error"] = "Sıra əlavə edilə bilmədi (məlumatları yoxlayın)"; }
            return RedirectToAction("Manage", new { id = templateId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int itemId, int templateId)
        {
            try { await _api.DeleteItemAsync(itemId); TempData["Success"] = "Sıra silindi"; }
            catch { TempData["Error"] = "Silinə bilmədi"; }
            return RedirectToAction("Manage", new { id = templateId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int id, bool active)
        {
            try { await _api.SetActiveAsync(id, active); TempData["Success"] = active ? "Aktivləşdirildi" : "Deaktiv edildi"; }
            catch { TempData["Error"] = "Status dəyişdirilə bilmədi"; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _api.DeleteAsync(id); TempData["Success"] = "Şablon silindi"; }
            catch { TempData["Error"] = "Silinə bilmədi"; }
            return RedirectToAction("Index");
        }
    }
}
