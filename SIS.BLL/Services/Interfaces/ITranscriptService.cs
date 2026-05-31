using SIS.BLL.Services.DTOs.Transcript;

namespace SIS.BLL.Services.Interfaces
{
    public interface ITranscriptService
    {
        Task<TranscriptDto> GenerateTranscriptAsync(int enrollmentId);
    }
}
