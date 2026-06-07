using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Public saytda göstərilən məzun uğur hekayəsi.
    /// Real Student entity ilə bağlılığı yoxdur — marketing kontent kimi ayrıca saxlanılır.
    /// </summary>
    public class PublicGraduateStory : BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string? GraduatedCourse { get; set; }
        public int? GraduationYear { get; set; }
        public string? CurrentPosition { get; set; }
        public string? Story { get; set; }

        public bool IsVisible { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }
}
