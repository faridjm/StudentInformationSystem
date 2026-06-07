using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Akademik Teacher-in public marketing profili.
    /// Teacher public saytda yalnız PublicTeacherProfile mövcud və IsVisible olduqda görünür.
    /// </summary>
    public class PublicTeacherProfile : BaseEntity
    {
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        public string? DisplayName { get; set; }
        public string? PublicTitle { get; set; }
        public string? Bio { get; set; }
        public string? Specialization { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LinkedInUrl { get; set; }

        public bool IsVisible { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }
}
