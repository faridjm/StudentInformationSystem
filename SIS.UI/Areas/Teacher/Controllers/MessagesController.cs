using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Chat;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class MessagesController : Controller
    {
        private readonly IChatApi _chatApi;
        private readonly IGroupApi _groupApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IStudentApi _studentApi;

        public MessagesController(IChatApi chatApi, IGroupApi groupApi,
            IEnrollmentApi enrollmentApi, IStudentApi studentApi)
        {
            _chatApi = chatApi;
            _groupApi = groupApi;
            _enrollmentApi = enrollmentApi;
            _studentApi = studentApi;
        }

        public async Task<IActionResult> Index(int? conversationId = null)
        {
            var conversations = (await _chatApi.GetConversationsAsync()).ToList();
            ViewBag.Conversations = conversations;
            ViewBag.ActiveConversationId = conversationId;

            if (conversationId.HasValue)
            {
                var messages = (await _chatApi.GetMessagesAsync(conversationId.Value)).ToList();
                await _chatApi.MarkAsReadAsync(conversationId.Value);
                ViewBag.Messages = messages;
            }

            return View();
        }

        // studentId = Student entity ID (not AppUserId)
        public async Task<IActionResult> Start(int groupId, int studentId)
        {
            var student = await _studentApi.GetByIdAsync(studentId);
            if (student == null) return RedirectToAction("Index");

            var conv = await _chatApi.StartConversationAsync(new StartConversationDto
            {
                GroupId = groupId,
                TargetUserId = student.AppUserId
            });
            return RedirectToAction("Index", new { conversationId = conv.Id });
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
