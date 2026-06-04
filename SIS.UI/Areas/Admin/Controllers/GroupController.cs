using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Groups;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class GroupController : Controller
    {
        private readonly IGroupApi _groupApi;
        private readonly ICourseApi _courseApi;
        private readonly ITeacherApi _teacherApi;
        private readonly IAcademicYearApi _yearApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly IAssessmentApi _assessmentApi;
        private readonly ICurriculumTemplateApi _templateApi;

        public GroupController(IGroupApi groupApi, ICourseApi courseApi, ITeacherApi teacherApi,
            IAcademicYearApi yearApi, IClassSessionApi sessionApi, IAssessmentApi assessmentApi,
            ICurriculumTemplateApi templateApi)
        {
            _groupApi = groupApi;
            _courseApi = courseApi;
            _teacherApi = teacherApi;
            _yearApi = yearApi;
            _sessionApi = sessionApi;
            _assessmentApi = assessmentApi;
            _templateApi = templateApi;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _groupApi.GetAllAsync();
            return View(groups);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Courses = await _courseApi.GetAllAsync();
            ViewBag.Teachers = await _teacherApi.GetAllAsync();
            ViewBag.Seasons = await _yearApi.GetAllAsync();
            try { ViewBag.Templates = (await _templateApi.GetAllAsync()).ToList(); }
            catch { ViewBag.Templates = new List<SIS.BLL.Services.DTOs.Curriculum.CurriculumTemplateDto>(); }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGroupDto dto)
        {
            try
            {
                var created = await _groupApi.CreateAsync(dto);
                TempData["Success"] = $"Qrup '{dto.Name}' yaradıldı";
                return RedirectToAction("Detail", new { id = created.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Courses = await _courseApi.GetAllAsync();
                ViewBag.Teachers = await _teacherApi.GetAllAsync();
                ViewBag.Seasons = await _yearApi.GetAllAsync();
                try { ViewBag.Templates = (await _templateApi.GetAllAsync()).ToList(); }
                catch { ViewBag.Templates = new List<SIS.BLL.Services.DTOs.Curriculum.CurriculumTemplateDto>(); }
                return View(dto);
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            var group = await _groupApi.GetByIdAsync(id);
            if (group == null) return NotFound();

            var sessions = await _sessionApi.GetByGroupAsync(id);
            var assessments = await _assessmentApi.GetByGroupAsync(id);

            ViewBag.Sessions = sessions.OrderBy(s => s.Date).ToList();
            ViewBag.Assessments = assessments.OrderBy(a => a.Date).ToList();
            return View(group);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var group = await _groupApi.GetByIdAsync(id);
            if (group == null) return NotFound();

            ViewBag.Teachers = await _teacherApi.GetAllAsync();
            ViewBag.Seasons = await _yearApi.GetAllAsync();
            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateGroupDto dto)
        {
            try
            {
                await _groupApi.UpdateAsync(dto);
                TempData["Success"] = "Qrup yeniləndi";
                return RedirectToAction("Detail", new { id = dto.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Edit", new { id = dto.Id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _groupApi.DeleteAsync(id);
            TempData["Success"] = "Qrup silindi";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTopic(int sessionId, int groupId, string? topic)
        {
            try
            {
                await _sessionApi.UpdateTopicAsync(sessionId,
                    new SIS.BLL.Services.DTOs.ClassSession.UpdateTopicDto { Topic = topic });
            }
            catch { }
            return RedirectToAction("Detail", new { id = groupId });
        }
    }
}
