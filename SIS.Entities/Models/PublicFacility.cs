using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Public saytın "Akademiya şəraiti" bölməsi (auditoriya, lab, mentor otağı və s.).
    /// Real dərs otağı/Room cədvəli ilə qarışdırılmır — yalnız marketing visual.
    /// </summary>
    public class PublicFacility : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public int? Capacity { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }
}
