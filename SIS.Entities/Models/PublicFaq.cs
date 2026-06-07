using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>Public saytda göstərilən FAQ (sual/cavab) elementi.</summary>
    public class PublicFaq : BaseEntity
    {
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public string? Category { get; set; }

        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }
}
