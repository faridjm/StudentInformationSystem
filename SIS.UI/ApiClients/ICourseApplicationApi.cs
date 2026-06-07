using Refit;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.Public;
using SIS.Entities.Enums;

namespace SIS.UI.ApiClients
{
    /// <summary>Public müraciət yaratma + Admin idarəetmə endpoint-ləri.</summary>
    public interface ICourseApplicationApi
    {
        // ---- Public (anonim) ----
        [Post("/api/public/applications")]
        Task<HttpResponseMessage> CreateAsync([Body] CreateCourseApplicationDto dto);

        // ---- Admin ----
        [Get("/api/courseapplications")]
        Task<PagedResult<CourseApplicationListDto>> ListAsync(
            [Query] string? search,
            [Query] CourseApplicationStatus? status,
            [Query] int? courseId,
            [Query] DateTime? fromDate,
            [Query] DateTime? toDate,
            [Query] int page = 1,
            [Query] int pageSize = 20);

        [Get("/api/courseapplications/stats")]
        Task<CourseApplicationStatsDto> StatsAsync();

        [Get("/api/courseapplications/{id}")]
        Task<CourseApplicationDetailDto> GetAsync(int id);

        [Put("/api/courseapplications/{id}/status")]
        Task UpdateStatusAsync(int id, [Body] UpdateApplicationStatusDto dto);

        [Put("/api/courseapplications/{id}/note")]
        Task UpdateNoteAsync(int id, [Body] UpdateApplicationNoteDto dto);

        [Post("/api/courseapplications/{id}/convert")]
        Task<ConvertApplicationResultDto> ConvertAsync(int id, [Body] ConvertApplicationToStudentDto dto);
    }
}
