using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Akademik Course-un public marketing profili.
    /// Course public saytda yalnız PublicCourseProfile mövcud və IsVisible olduqda görünür.
    /// </summary>
    public class PublicCourseProfile : BaseEntity
    {
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public string? PublicTitle { get; set; }
        public string? PublicShortDescription { get; set; }
        public string? PublicDescription { get; set; }
        public string? CoverImageUrl { get; set; }

        public bool IsVisible { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        public string? WhatYouWillLearn { get; set; }
        public string? CareerOutcomes { get; set; }
    }
}
