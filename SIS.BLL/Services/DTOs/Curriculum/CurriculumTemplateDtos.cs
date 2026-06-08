using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Curriculum
{
    public class CurriculumTemplateDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }
        public int AssessmentCount { get; set; }
    }

    public class CurriculumTemplateDetailDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<CurriculumTemplateItemDto> Items { get; set; } = new();
    }

    public class CurriculumTemplateItemDto
    {
        public int Id { get; set; }
        public int SessionNumber { get; set; }
        public string Topic { get; set; } = null!;
        public bool HasAssessment { get; set; }
        public string? AssessmentTitle { get; set; }
        public AssessmentType? AssessmentType { get; set; }
        public decimal? WeightPercent { get; set; }
        public decimal? MaxScore { get; set; }
    }

    public class CreateCurriculumTemplateDto
    {
        public int CourseId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateCurriculumTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CreateTemplateItemDto
    {
        public int CurriculumTemplateId { get; set; }
        public int SessionNumber { get; set; }
        public string Topic { get; set; } = null!;
        public bool HasAssessment { get; set; }
        public string? AssessmentTitle { get; set; }
        public AssessmentType? AssessmentType { get; set; }
        public decimal? WeightPercent { get; set; }
        public decimal? MaxScore { get; set; }
    }

    public class UpdateTemplateItemDto
    {
        public int Id { get; set; }
        public int CurriculumTemplateId { get; set; }
        public int SessionNumber { get; set; }
        public string Topic { get; set; } = null!;
        public bool HasAssessment { get; set; }
        public string? AssessmentTitle { get; set; }
        public AssessmentType? AssessmentType { get; set; }
        public decimal? WeightPercent { get; set; }
        public decimal? MaxScore { get; set; }
    }

    // ── Bulk session/item əlavəsi (Scalar/Swagger üçün) ──

    public class BulkTemplateItemDto
    {
        public int SessionNumber { get; set; }
        public string Topic { get; set; } = null!;
        public bool HasAssessment { get; set; }
        public string? AssessmentTitle { get; set; }
        public AssessmentType? AssessmentType { get; set; }
        public decimal? WeightPercent { get; set; }
        public decimal? MaxScore { get; set; }
    }

    public class BulkTemplateItemsDto
    {
        /// <summary>"replace" — mövcud sıraları sil və yenidən yaz; "append" (default) — üstünə əlavə et.</summary>
        public string? Mode { get; set; }
        public List<BulkTemplateItemDto> Sessions { get; set; } = new();
    }
}
