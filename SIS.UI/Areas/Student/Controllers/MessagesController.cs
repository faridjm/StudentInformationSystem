using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Chat;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class MessagesController : Controller
    {
        private readonly IChatApi _chatApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly ITeacherApi _teacherApi;

        public MessagesController(IChatApi chatApi, IEnrollmentApi enrollmentApi, ITeacherApi teacherApi)
        {
            _chatApi = chatApi;
            _enrollmentApi = enrollmentApi;
            _teacherApi = teacherApi;
        }

        public async Task<IActionResult> Index(int? conversationId = null, string? prefill = null)
        {
            var conversations = (await _chatApi.GetConversationsAsync()).ToList();
            ViewBag.Conversations = conversations;
            ViewBag.ActiveConversationId = conversationId;
            ViewBag.Prefill = prefill;

            if (conversationId.HasValue)
            {
                var messages = (await _chatApi.GetMessagesAsync(conversationId.Value)).ToList();
                await _chatApi.MarkAsReadAsync(conversationId.Value);
                ViewBag.Messages = messages;
            }

            return View();
        }

        // teacherId = Teacher entity ID (not AppUserId), prefill = optional context text
        public async Task<IActionResult> Start(int groupId, int teacherId, string? prefill = null)
        {
            var teacher = await _teacherApi.GetByIdAsync(teacherId);
            if (teacher == null) return RedirectToAction("Index");

            var conv = await _chatApi.StartConversationAsync(new StartConversationDto
            {
                GroupId = groupId,
                TargetUserId = teacher.AppUserId
            });
            return RedirectToAction("Index", new { conversationId = conv.Id, prefill });
        }

        [HttpPost]
        public async Task<IActionResult> Send(int conversationId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
                await _chatApi.SendMessageAsync(conversationId, new SendMessageDto { ConversationId = conversationId, Content = content });
            return RedirectToAction("Index", new { conversationId });
        }
    }
}
