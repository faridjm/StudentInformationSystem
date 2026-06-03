using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Material
{
    public class CourseMaterialDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public MaterialType MaterialType { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public string? LinkUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCourseMaterialDto
    {
        public int GroupId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public MaterialType MaterialType { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public string? LinkUrl { get; set; }
    }

    public class UpdateCourseMaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public MaterialType MaterialType { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public string? LinkUrl { get; set; }
    }
}
