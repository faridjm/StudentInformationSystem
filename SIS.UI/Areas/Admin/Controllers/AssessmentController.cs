using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assessment;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AssessmentController : Controller
    {
        private readonly IAssessmentApi _assessmentApi;

        public AssessmentController(IAssessmentApi assessmentApi)
        {
            _assessmentApi = assessmentApi;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssessmentItemDto dto)
        {
            try
            {
                await _assessmentApi.CreateAsync(dto);
                TempData["Success"] = "Qiymətləndirmə elementi əlavə edildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detail", "Group", new { id = dto.GroupId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int returnGroupId)
        {
            try
            {
                await _assessmentApi.DeleteAsync(id);
                TempData["Success"] = "Silindi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detail", "Group", new { id = returnGroupId });
        }
    }
}
