using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class ScheduleController : Controller
    {
        private readonly IGroupApi _groupApi;
        private readonly IClassSessionApi _sessionApi;

        public ScheduleController(IGroupApi groupApi, IClassSessionApi sessionApi)
        {
            _groupApi = groupApi;
            _sessionApi = sessionApi;
        }

        public async Task<IActionResult> Index()
            => View(await _groupApi.GetMyGroupsAsync());

        public async Task<IActionResult> Sessions(int groupId)
        {
            var sessions = await _sessionApi.GetByGroupAsync(groupId);
            ViewBag.GroupId = groupId;
            return View(sessions);
        }
    }
}
