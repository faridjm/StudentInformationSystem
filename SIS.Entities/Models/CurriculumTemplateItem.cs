using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Şablon sırası: hər SessionNumber üçün mövzu və istəyə bağlı qiymətləndirmə.
    /// Məs: 35 | Quiz 1 | Quiz | 10% | 100.
    /// </summary>
    public class CurriculumTemplateItem : BaseEntity
    {
        public int CurriculumTemplateId { get; set; }
        public int SessionNumber { get; set; }
        public string Topic { get; set; } = null!;

        public bool HasAssessment { get; set; }
        public string? AssessmentTitle { get; set; }
        public AssessmentType? AssessmentType { get; set; }
        public decimal? WeightPercent { get; set; }
        public decimal? MaxScore { get; set; }

        public CurriculumTemplate CurriculumTemplate { get; set; } = null!;
    }
}
