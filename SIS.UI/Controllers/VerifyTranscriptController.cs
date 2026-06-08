using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Transcript;
using SIS.UI.ApiClients;

namespace SIS.UI.Controllers
{
    /// <summary>
    /// Public transcript doğrulama səhifəsi — login tələb etmir.
    /// QR kod /VerifyTranscript/{code} ünvanına yönəlir.
    /// </summary>
    [AllowAnonymous]
    public class VerifyTranscriptController : Controller
    {
        private readonly ITranscriptApi _transcriptApi;

        public VerifyTranscriptController(ITranscriptApi transcriptApi)
        {
            _transcriptApi = transcriptApi;
        }

        [HttpGet("/VerifyTranscript/{code?}")]
        public async Task<IActionResult> Index(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return View(new TranscriptVerificationPublicDto { Status = "Invalid" });

            try
            {
                var result = await _transcriptApi.VerifyAsync(code.Trim());
                return View(result ?? new TranscriptVerificationPublicDto { Status = "Invalid", VerificationCode = code });
            }
            catch
            {
                // API əlçatmaz və ya kod tapılmadı → Invalid göstər
                return View(new TranscriptVerificationPublicDto { Status = "Invalid", VerificationCode = code });
            }
        }
    }
}
