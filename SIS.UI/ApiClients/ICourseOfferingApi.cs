using Refit;
using SIS.BLL.Services.DTOs.CourseOffering;

namespace SIS.UI.ApiClients
{
    public interface ICourseOfferingApi
    {
        [Get("/api/courseoffering")]
        Task<IEnumerable<CourseOfferingListDto>> GetAllAsync();

        [Get("/api/courseoffering/{id}")]
        Task<CourseOfferingDetailDto> GetByIdAsync(int id);

        [Get("/api/courseoffering/active")]
        Task<IEnumerable<CourseOfferingListDto>> GetActiveAsync();

        [Get("/api/courseoffering/teacher/{teacherId}")]
        Task<IEnumerable<CourseOfferingListDto>> GetByTeacherAsync(int teacherId);

        [Get("/api/courseoffering/term/{termId}")]
        Task<IEnumerable<CourseOfferingListDto>> GetByTermAsync(int termId);

        [Post("/api/courseoffering")]
        Task CreateAsync([Body] CreateCourseOfferingDto dto);

        [Put("/api/courseoffering")]
        Task UpdateAsync([Body] UpdateCourseOfferingDto dto);

        [Delete("/api/courseoffering/{id}")]
        Task DeleteAsync(int id);
    }


}
