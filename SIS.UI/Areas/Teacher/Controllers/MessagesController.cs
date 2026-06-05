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
        private readonly IWebHostEnvironment _env;

        public MessagesController(IChatApi chatApi, IGroupApi groupApi,
            IEnrollmentApi enrollmentApi, IStudentApi studentApi, IWebHostEnvironment env)
        {
            _chatApi = chatApi;
            _groupApi = groupApi;
            _enrollmentApi = enrollmentApi;
            _studentApi = studentApi;
            _env = env;
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

        /// <summary>AJAX (JSON) — text mesaj göndərmək üçün. Page reload olmur.</summary>
        [HttpPost]
        public async Task<IActionResult> SendAjax(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(new { error = "Mesaj boş ola bilməz" });
            try
            {
                var msg = await _chatApi.SendMessageAsync(conversationId,
                    new SendMessageDto { ConversationId = conversationId, Content = content });
                return Json(msg);
            }
            catch (Refit.ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new { error = ex.Message });
            }
        }

        /// <summary>AJAX upload — fayl UI-da saxlanılır, sonra API-yə yalnız JSON metadata göndərilir.</summary>
        [HttpPost]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> Upload(
            [FromForm] int conversationId,
            [FromForm] string type,
            [FromForm] string? content,
            [FromForm] int? durationSeconds,
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Fayl seçilməyib" });

            string subfolder = type?.ToLowerInvariant() switch
            {
                "image" => "images",
                "file"  => "files",
                "voice" => "voices",
                _       => null!
            };
            if (subfolder == null)
                return BadRequest(new { error = "Növ səhvdir" });

            var blocked = new[] { ".exe", ".bat", ".cmd", ".ps1", ".dll", ".msi", ".vbs", ".scr" };
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            if (blocked.Contains(ext))
                return BadRequest(new { error = "Bu fayl tipi qadağandır" });

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dir = Path.Combine(webRoot, "uploads", "chat", subfolder);
            Directory.CreateDirectory(dir);

            var safeName = Guid.NewGuid().ToString("N") + ext;
            var fullPath = Path.Combine(dir, safeName);
            await using (var fs = System.IO.File.Create(fullPath))
                await file.CopyToAsync(fs);

            var url = $"/uploads/chat/{subfolder}/{safeName}";

            var atype = type.ToLowerInvariant() switch
            {
                "image" => SIS.Entities.Enums.ChatAttachmentType.Image,
                "file"  => SIS.Entities.Enums.ChatAttachmentType.File,
                "voice" => SIS.Entities.Enums.ChatAttachmentType.Voice,
                _       => SIS.Entities.Enums.ChatAttachmentType.None
            };

            try
            {
                var msg = await _chatApi.SendAttachmentMetaAsync(conversationId, new SendAttachmentMessageDto
                {
                    ConversationId = conversationId,
                    Content = content,
                    AttachmentType = atype,
                    AttachmentUrl = url,
                    AttachmentFileName = SanitizeFileName(file.FileName),
                    AttachmentContentType = file.ContentType,
                    AttachmentSize = file.Length,
                    DurationSeconds = atype == SIS.Entities.Enums.ChatAttachmentType.Voice ? durationSeconds : null
                });
                return Json(msg);
            }
            catch (Refit.ApiException ex)
            {
                var apiBody = ex.Content ?? "(empty body)";
                return StatusCode((int)ex.StatusCode, new
                {
                    error = $"API {(int)ex.StatusCode}: {ex.ReasonPhrase}",
                    apiBody,
                    requestUrl = $"/api/chat/conversations/{conversationId}/messages/attachment-meta",
                    storedUrl = url
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.GetType().Name + ": " + ex.Message, stack = ex.StackTrace });
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "file";
            var name = Path.GetFileName(fileName);
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Length > 200 ? name.Substring(0, 200) : name;
        }
    }
}
