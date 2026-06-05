using Refit;
using SIS.BLL.Services.DTOs.Curriculum;

namespace SIS.UI.ApiClients
{
    public interface ICurriculumTemplateApi
    {
        [Get("/api/curriculumtemplate")]
        Task<IEnumerable<CurriculumTemplateDto>> GetAllAsync();

        [Get("/api/curriculumtemplate/course/{courseId}")]
        Task<IEnumerable<CurriculumTemplateDto>> GetByCourseAsync(int courseId);

        [Get("/api/curriculumtemplate/{id}")]
        Task<CurriculumTemplateDetailDto> GetDetailAsync(int id);

        [Post("/api/curriculumtemplate")]
        Task<CurriculumTemplateDto> CreateAsync([Body] CreateCurriculumTemplateDto dto);

        [Put("/api/curriculumtemplate")]
        Task UpdateAsync([Body] UpdateCurriculumTemplateDto dto);

        [Post("/api/curriculumtemplate/{id}/active")]
        Task SetActiveAsync(int id, bool active);

        [Delete("/api/curriculumtemplate/{id}")]
        Task DeleteAsync(int id);

        [Post("/api/curriculumtemplate/items")]
        Task<CurriculumTemplateItemDto> AddItemAsync([Body] CreateTemplateItemDto dto);

        [Put("/api/curriculumtemplate/items")]
        Task<CurriculumTemplateItemDto> UpdateItemAsync([Body] UpdateTemplateItemDto dto);

        [Delete("/api/curriculumtemplate/items/{itemId}")]
        Task DeleteItemAsync(int itemId);

        [Get("/api/curriculumtemplate/{templateId}/next-session-number")]
        Task<NextSessionNumberResponse> GetNextSessionNumberAsync(int templateId);
    }

    public class NextSessionNumberResponse
    {
        public int NextSessionNumber { get; set; }
    }
}
