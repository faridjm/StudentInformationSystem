using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Qrupa aid tədris materialı (fayl, link, video və s.).
    /// IsActive = göstər/gizlət; IsDeleted (BaseEntity) = soft delete.
    /// </summary>
    public class CourseMaterial : BaseEntity
    {
        public int GroupId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public MaterialType MaterialType { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public string? LinkUrl { get; set; }
        public int UploadedByTeacherId { get; set; }
        public bool IsActive { get; set; } = true;

        public Group Group { get; set; } = null!;
    }
}
