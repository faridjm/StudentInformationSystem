using Refit;
using SIS.BLL.Services.DTOs.ClassSession;

namespace SIS.UI.ApiClients
{
    public interface IClassSessionApi
    {
        [Get("/api/classsession/{id}")]
        Task<ClassSessionDto> GetByIdAsync(int id);

        [Get("/api/classsession/course/{courseOfferingId}")]
        Task<IEnumerable<ClassSessionDto>> GetByCourseAsync(int courseOfferingId);

        [Get("/api/classsession/course/{courseOfferingId}/range")]
        Task<IEnumerable<ClassSessionDto>> GetByCourseRangeAsync(
            int courseOfferingId, [Query] DateOnly from, [Query] DateOnly to);

        [Get("/api/classsession/student/{studentId}/range")]
        Task<IEnumerable<ClassSessionDto>> GetByStudentRangeAsync(
            int studentId, [Query] DateOnly from, [Query] DateOnly to);

        [Post("/api/classsession/generate/{courseOfferingId}")]
        Task<object> GenerateAsync(int courseOfferingId);

        [Post("/api/classsession")]
        Task<object> CreateAsync([Body] CreateClassSessionDto dto);

        [Put("/api/classsession")]
        Task UpdateAsync([Body] UpdateClassSessionDto dto);

        [Post("/api/classsession/cancel")]
        Task CancelAsync([Body] CancelClassSessionDto dto);

        [Post("/api/classsession/makeup")]
        Task<object> CreateMakeupAsync([Body] CreateMakeupSessionDto dto);

        [Delete("/api/classsession/{id}")]
        Task DeleteAsync(int id);
    }
}
