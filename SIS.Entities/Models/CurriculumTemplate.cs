using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Kursa aid təkrar istifadə oluna bilən dərs cədvəli şablonu (SessionNumber əsaslı, konkret tarixsiz).
    /// </summary>
    public class CurriculumTemplate : BaseEntity
    {
        public int CourseId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public Course Course { get; set; } = null!;
        public ICollection<CurriculumTemplateItem> Items { get; set; } = new List<CurriculumTemplateItem>();
    }
}
