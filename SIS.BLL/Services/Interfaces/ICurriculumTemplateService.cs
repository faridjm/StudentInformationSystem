using SIS.BLL.Services.DTOs.Curriculum;

namespace SIS.BLL.Services.Interfaces
{
    public interface ICurriculumTemplateService
    {
        Task<IEnumerable<CurriculumTemplateDto>> GetAllAsync();
        Task<IEnumerable<CurriculumTemplateDto>> GetByCourseAsync(int courseId);
        Task<CurriculumTemplateDetailDto?> GetDetailAsync(int id);
        Task<CurriculumTemplateDto?> GetActiveByCourseAsync(int courseId);
        Task<CurriculumTemplateDto> CreateAsync(CreateCurriculumTemplateDto dto);
        Task UpdateAsync(UpdateCurriculumTemplateDto dto);
        Task SetActiveAsync(int id, bool active);
        Task DeleteAsync(int id);

        Task<CurriculumTemplateItemDto> AddItemAsync(CreateTemplateItemDto dto);

        /// <summary>Bir şablona çoxlu sıra (session) əlavə edir. mode: "replace" | "append" (default).</summary>
        Task<List<CurriculumTemplateItemDto>> AddItemsBulkAsync(int templateId, BulkTemplateItemsDto dto);

        Task<CurriculumTemplateItemDto> UpdateItemAsync(UpdateTemplateItemDto dto);
        Task DeleteItemAsync(int itemId);

        /// <summary>Şablonda növbəti boş SessionNumber. Heç sıra yoxdursa 1 qaytarır.</summary>
        Task<int> GetNextSessionNumberAsync(int templateId);

        /// <summary>Şablonu qrupa tətbiq edir: mövzuları ClassSession-lara, qiymətləndirmələri AssessmentItem-ə yazır.</summary>
        Task ApplyToGroupAsync(int groupId, int templateId);
    }
}
