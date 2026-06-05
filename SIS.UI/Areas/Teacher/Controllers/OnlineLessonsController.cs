using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Entities.Enums;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class OnlineLessonsController : Controller
    {
        private readonly IOnlineLessonApi _api;
        public OnlineLessonsController(IOnlineLessonApi api) { _api = api; }

        public async Task<IActionResult> Classroom(int id)
        {
            try
            {
                var lesson = await _api.GetByIdAsync(id);

                if (lesson.Status == OnlineLessonStatus.Ended || lesson.Status == OnlineLessonStatus.Cancelled)
                {
                    TempData["Error"] = lesson.Status == OnlineLessonStatus.Ended
                        ? "Bu online dərs artıq bitib"
                        : "Bu online dərs ləğv edilib";
                    return RedirectToAction("Index", "Courses");
                }

                ViewBag.IsTeacher = true;
                return View("~/Views/Shared/_OnlineClassroom.cshtml", lesson);
            }
            catch (Refit.ApiException)
            {
                TempData["Error"] = "Bu classroom-a giriş icazəniz yoxdur";
                return RedirectToAction("Index", "Courses");
            }
        }
    }
}
