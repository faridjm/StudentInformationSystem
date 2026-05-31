using Refit;

namespace SIS.UI.ApiClients
{
    public interface ITranscriptApi
    {
        [Get("/api/transcript/{enrollmentId}/pdf")]
        Task<HttpResponseMessage> GetPdfAsync(int enrollmentId);
    }
}
