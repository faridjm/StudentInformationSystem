using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIS.UI.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions =
            [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".txt", ".pptx"];

        // Materiallar üçün: təhlükəli uzantılar bloklanır, qalanına icazə verilir (denylist)
        private static readonly string[] BlockedExtensions =
            [".exe", ".bat", ".cmd", ".ps1", ".dll", ".com", ".scr", ".msi", ".vbs"];

        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private const long MaxMaterialFileSize = 25 * 1024 * 1024; // 25 MB

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> File(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Fayl seçilmədi" });

            if (file.Length > MaxFileSize)
                return BadRequest(new { error = "Fayl 10 MB-dan böyük ola bilməz" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest(new { error = $"Bu fayl növünə icazə verilmir. İcazə veriləns: {string.Join(", ", AllowedExtensions)}" });

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return Ok(new
            {
                url = $"/uploads/{fileName}",
                name = file.FileName,
                size = file.Length
            });
        }

        // Tədris materialları üçün yükləmə (denylist + 25 MB limit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> MaterialFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Fayl seçilmədi" });

            if (file.Length > MaxMaterialFileSize)
                return BadRequest(new { error = "Fayl 25 MB-dan böyük ola bilməz" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (BlockedExtensions.Contains(ext))
                return BadRequest(new { error = "Bu fayl növünə icazə verilmir" });

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "materials");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return Ok(new
            {
                url = $"/uploads/materials/{fileName}",
                name = file.FileName,
                size = file.Length
            });
        }
    }
}
