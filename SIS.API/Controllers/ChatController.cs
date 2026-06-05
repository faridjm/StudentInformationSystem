using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Chat;
using SIS.BLL.Services.Interfaces;
using SIS.Entities.Enums;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICurrentUser _currentUser;
        private readonly IWebHostEnvironment _env;

        // Allowed file extensions
        private static readonly HashSet<string> ImageExt =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private static readonly HashSet<string> FileExt =
            new(StringComparer.OrdinalIgnoreCase) {
                ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
                ".zip", ".txt", ".csv", ".cs", ".js", ".html", ".css", ".json"
            };

        private static readonly HashSet<string> BlockedExt =
            new(StringComparer.OrdinalIgnoreCase) {
                ".exe", ".bat", ".cmd", ".ps1", ".dll", ".msi", ".vbs", ".scr", ".com", ".sh"
            };

        private static readonly HashSet<string> VoiceExt =
            new(StringComparer.OrdinalIgnoreCase) { ".webm", ".ogg", ".mp3", ".wav", ".m4a" };

        private const long MaxImageSize = 10 * 1024 * 1024;  // 10 MB
        private const long MaxFileSize  = 25 * 1024 * 1024;  // 25 MB
        private const long MaxVoiceSize = 5  * 1024 * 1024;  //  5 MB

        public ChatController(IChatService chatService, ICurrentUser currentUser, IWebHostEnvironment env)
        {
            _chatService = chatService;
            _currentUser = currentUser;
            _env = env;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = _currentUser.UserId!;
            var conversations = await _chatService.GetMyConversationsAsync(userId);
            return Ok(conversations);
        }

        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationDto dto)
        {
            var userId = _currentUser.UserId!;
            var conv = await _chatService.GetOrCreateConversationAsync(userId, dto.GroupId, dto.TargetUserId);
            return Ok(conv);
        }

        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(int id, int take = 50, int skip = 0)
        {
            var userId = _currentUser.UserId!;
            if (!await _chatService.HasAccessAsync(id, userId))
                return Forbid();
            var messages = await _chatService.GetMessagesAsync(id, userId, take, skip);
            return Ok(messages);
        }

        [HttpPost("conversations/{id}/messages")]
        public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
        {
            var userId = _currentUser.UserId!;
            if (!await _chatService.HasAccessAsync(id, userId)) return Forbid();
            var msg = await _chatService.SendMessageAsync(userId, id, dto.Content ?? "");
            return Ok(msg);
        }

        /// <summary>
        /// Attachment metadata (JSON) — fayl artıq UI tərəfində saxlanılıb,
        /// burada yalnız URL + metadata DB-yə yazılır və SignalR fan-out olur.
        /// </summary>
        [HttpPost("conversations/{id}/messages/attachment-meta")]
        public async Task<IActionResult> SendAttachmentMeta(int id, [FromBody] SendAttachmentMessageDto dto)
        {
            var userId = _currentUser.UserId!;
            if (!await _chatService.HasAccessAsync(id, userId)) return Forbid();

            dto.ConversationId = id;
            if (dto.AttachmentType == ChatAttachmentType.None)
                return BadRequest(new { message = "Attachment növü düzgün deyil" });
            if (string.IsNullOrEmpty(dto.AttachmentUrl))
                return BadRequest(new { message = "Attachment URL boşdur" });

            var msg = await _chatService.SendAttachmentMessageAsync(userId, dto);
            return Ok(msg);
        }

        /// <summary>
        /// Image / File / Voice mesaj göndərir. multipart/form-data:
        /// fields: type=Image|File|Voice, content (optional), durationSeconds (Voice üçün), file (IFormFile)
        /// </summary>
        [HttpPost("conversations/{id}/messages/attachment")]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> SendAttachment(
            int id,
            [FromForm] string type,
            [FromForm] string? content,
            [FromForm] int? durationSeconds,
            [FromForm] IFormFile file)
        {
            var userId = _currentUser.UserId!;
            if (!await _chatService.HasAccessAsync(id, userId)) return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Fayl seçilməyib" });

            if (!Enum.TryParse<ChatAttachmentType>(type, true, out var atype) || atype == ChatAttachmentType.None)
                return BadRequest(new { message = "Attachment növü düzgün deyil" });

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";

            if (BlockedExt.Contains(ext))
                return BadRequest(new { message = "Bu fayl tipi qadağandır" });

            string subfolder;
            long maxSize;
            switch (atype)
            {
                case ChatAttachmentType.Image:
                    if (!ImageExt.Contains(ext))
                        return BadRequest(new { message = "Şəkil formatı qəbul edilmir (jpg, png, webp, gif)" });
                    subfolder = "images";
                    maxSize = MaxImageSize;
                    break;
                case ChatAttachmentType.File:
                    if (!FileExt.Contains(ext))
                        return BadRequest(new { message = "Bu fayl tipi qəbul edilmir" });
                    subfolder = "files";
                    maxSize = MaxFileSize;
                    break;
                case ChatAttachmentType.Voice:
                    if (!VoiceExt.Contains(ext) && !file.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                        return BadRequest(new { message = "Səs formatı qəbul edilmir" });
                    subfolder = "voices";
                    maxSize = MaxVoiceSize;
                    break;
                default:
                    return BadRequest(new { message = "Növ səhvdir" });
            }

            if (file.Length > maxSize)
                return BadRequest(new { message = $"Fayl ölçüsü {maxSize / (1024 * 1024)} MB-dən çox ola bilməz" });

            // Save file to wwwroot/uploads/chat/{subfolder}
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dir = Path.Combine(webRoot, "uploads", "chat", subfolder);
            Directory.CreateDirectory(dir);

            var safeName = Guid.NewGuid().ToString("N") + (string.IsNullOrEmpty(ext) ? "" : ext);
            var fullPath = Path.Combine(dir, safeName);

            await using (var stream = System.IO.File.Create(fullPath))
                await file.CopyToAsync(stream);

            var url = $"/uploads/chat/{subfolder}/{safeName}";

            var msg = await _chatService.SendAttachmentMessageAsync(userId, new SendAttachmentMessageDto
            {
                ConversationId = id,
                Content = content,
                AttachmentType = atype,
                AttachmentUrl = url,
                AttachmentFileName = SanitizeFileName(file.FileName),
                AttachmentContentType = file.ContentType,
                AttachmentSize = file.Length,
                DurationSeconds = atype == ChatAttachmentType.Voice ? durationSeconds : null
            });
            return Ok(msg);
        }

        [HttpPut("conversations/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _currentUser.UserId!;
            await _chatService.MarkAsReadAsync(id, userId);
            return Ok();
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _currentUser.UserId!;
            var count = await _chatService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        // ── Helpers ──
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
