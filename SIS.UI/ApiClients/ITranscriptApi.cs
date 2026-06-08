using Refit;
using SIS.BLL.Services.DTOs.Transcript;

namespace SIS.UI.ApiClients
{
    public interface ITranscriptApi
    {
        [Get("/api/transcript/{enrollmentId}")]
        Task<TranscriptDto> GetAsync(int enrollmentId);

        [Get("/api/transcript/{enrollmentId}/pdf")]
        Task<HttpResponseMessage> GetPdfAsync(int enrollmentId, string? lang = "az");

        [Post("/api/transcript/{enrollmentId}/email")]
        Task<HttpResponseMessage> SendByEmailAsync(int enrollmentId, string? lang = "az");

        [Get("/api/transcript/verify/{code}")]
        Task<TranscriptVerificationPublicDto> VerifyAsync(string code);
    }
}
