using SIS.BLL.Services.DTOs.Material;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class CourseMaterialService : ICourseMaterialService
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] DangerousExtensions = { ".exe", ".bat", ".cmd", ".ps1", ".dll" };

        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notifications;
        private readonly IAuditLogService _audit;

        public CourseMaterialService(IUnitOfWork unitOfWork, INotificationService notifications, IAuditLogService audit)
        {
            _unitOfWork = unitOfWork;
            _notifications = notifications;
            _audit = audit;
        }

        public async Task<IEnumerable<CourseMaterialDto>> GetByGroupAsync(int groupId)
            => (await _unitOfWork.CourseMaterials.GetByGroupAsync(groupId)).Select(MapToDto);

        public async Task<IEnumerable<CourseMaterialDto>> GetVisibleByGroupAsync(int groupId)
            => (await _unitOfWork.CourseMaterials.GetVisibleByGroupAsync(groupId)).Select(MapToDto);

        public async Task<CourseMaterialDto?> GetByIdAsync(int id)
        {
            var m = await _unitOfWork.CourseMaterials.GetByIdAsync(id);
            return m == null ? null : MapToDto(m);
        }

        public async Task<CourseMaterialDto> CreateAsync(CreateCourseMaterialDto dto, int uploadedByTeacherId)
        {
            Validate(dto.Title, dto.MaterialType, dto.LinkUrl, dto.FilePath, dto.FileName, dto.FileSize);

            var now = DateTime.UtcNow;
            var material = new CourseMaterial
            {
                GroupId = dto.GroupId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                MaterialType = dto.MaterialType,
                FilePath = dto.FilePath,
                FileName = dto.FileName,
                FileSize = dto.FileSize,
                LinkUrl = dto.LinkUrl,
                UploadedByTeacherId = uploadedByTeacherId,
                IsActive = true,
                CreatedAt = now
            };
            await _unitOfWork.CourseMaterials.AddAsync(material);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("CreateMaterial", "CourseMaterial", material.Id.ToString(),
                $"Material əlavə edildi: {material.Title} (Group #{material.GroupId})");

            // Qrupun tələbələrinə bildiriş
            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(dto.GroupId);
            if (group != null)
            {
                var studentUserIds = group.Enrollments
                    .Where(e => !e.IsDeleted && e.Student != null && e.Student.AppUserId != null)
                    .Select(e => e.Student.AppUserId);
                await _notifications.SendToManyAsync(studentUserIds,
                    "New Learning Material",
                    $"{material.Title} has been added for {group.Name}",
                    NotificationType.AnnouncementPosted,
                    $"/Student/Courses/MaterialsByGroup?groupId={material.GroupId}");
            }

            return MapToDto(material);
        }

        public async Task UpdateAsync(UpdateCourseMaterialDto dto)
        {
            var material = await _unitOfWork.CourseMaterials.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Material tapılmadı");

            Validate(dto.Title, dto.MaterialType, dto.LinkUrl, dto.FilePath, dto.FileName, dto.FileSize);

            material.Title = dto.Title.Trim();
            material.Description = dto.Description;
            material.MaterialType = dto.MaterialType;
            material.FilePath = dto.FilePath;
            material.FileName = dto.FileName;
            material.FileSize = dto.FileSize;
            material.LinkUrl = dto.LinkUrl;
            _unitOfWork.CourseMaterials.Update(material);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("UpdateMaterial", "CourseMaterial", material.Id.ToString(),
                $"Material yeniləndi: {material.Title}");
        }

        public async Task SetActiveAsync(int id, bool active)
        {
            var material = await _unitOfWork.CourseMaterials.GetByIdAsync(id)
                ?? throw new NotFoundException("Material tapılmadı");

            material.IsActive = active;
            _unitOfWork.CourseMaterials.Update(material);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync(active ? "ShowMaterial" : "HideMaterial", "CourseMaterial", material.Id.ToString(),
                $"Material {(active ? "göstərildi" : "gizlədildi")}: {material.Title}");
        }

        public async Task DeleteAsync(int id)
        {
            var material = await _unitOfWork.CourseMaterials.GetByIdAsync(id)
                ?? throw new NotFoundException("Material tapılmadı");

            _unitOfWork.CourseMaterials.SoftDelete(material);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("DeleteMaterial", "CourseMaterial", material.Id.ToString(),
                $"Material silindi: {material.Title}");
        }

        private static void Validate(string title, MaterialType type, string? linkUrl, string? filePath, string? fileName, long? fileSize)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new BadRequestException("Başlıq tələb olunur");

            bool isLink = type is MaterialType.Link or MaterialType.Video;
            bool isFile = type is MaterialType.File or MaterialType.Document or MaterialType.Presentation or MaterialType.Code;

            if (isLink && string.IsNullOrWhiteSpace(linkUrl))
                throw new BadRequestException("Link/Video üçün keçid (URL) tələb olunur");

            if (isFile && string.IsNullOrWhiteSpace(filePath))
                throw new BadRequestException("Bu material növü üçün fayl tələb olunur");

            if (type == MaterialType.Other && string.IsNullOrWhiteSpace(linkUrl) && string.IsNullOrWhiteSpace(filePath))
                throw new BadRequestException("Fayl və ya keçid (URL) daxil edin");

            if (fileSize.HasValue && fileSize.Value > MaxFileSize)
                throw new BadRequestException("Fayl 10 MB-dan böyük ola bilməz");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
                if (DangerousExtensions.Contains(ext))
                    throw new BadRequestException("Bu fayl növünə icazə verilmir");
            }
        }

        private static CourseMaterialDto MapToDto(CourseMaterial m) => new()
        {
            Id = m.Id,
            GroupId = m.GroupId,
            Title = m.Title,
            Description = m.Description,
            MaterialType = m.MaterialType,
            FilePath = m.FilePath,
            FileName = m.FileName,
            FileSize = m.FileSize,
            LinkUrl = m.LinkUrl,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };
    }
}
