using Refit;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.DTOs.Parent;
using SIS.BLL.Services.DTOs.Transcript;

namespace SIS.UI.ApiClients
{
    public interface IParentApi
    {
        [Get("/api/parent/children")]
        Task<IEnumerable<ParentChildDto>> GetChildrenAsync();

        [Get("/api/parent/children/{studentId}/enrollments")]
        Task<IEnumerable<EnrollmentListDto>> GetEnrollmentsAsync(int studentId);

        [Get("/api/parent/children/{studentId}/transcript/{enrollmentId}")]
        Task<TranscriptDto> GetTranscriptAsync(int studentId, int enrollmentId);

        [Get("/api/parent/children/{studentId}/announcements")]
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsAsync(int studentId);

        [Get("/api/parent/children/{studentId}/quiz-results")]
        Task<IEnumerable<ParentQuizResultDto>> GetQuizResultsAsync(int studentId);

        [Get("/api/parent/children/{studentId}/transcript/{enrollmentId}/pdf")]
        Task<HttpResponseMessage> GetTranscriptPdfAsync(int studentId, int enrollmentId, string? lang = "az");
    }
}
