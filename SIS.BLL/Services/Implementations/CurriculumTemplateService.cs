using SIS.BLL.Services.DTOs.Curriculum;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class CurriculumTemplateService : ICurriculumTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _audit;

        public CurriculumTemplateService(IUnitOfWork unitOfWork, IAuditLogService audit)
        {
            _unitOfWork = unitOfWork;
            _audit = audit;
        }

        // ── Templates ──
        public async Task<IEnumerable<CurriculumTemplateDto>> GetAllAsync()
        {
            var templates = await _unitOfWork.CurriculumTemplates.GetAllAsync();
            var courses = (await _unitOfWork.Courses.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
            return templates.Select(t => MapToDto(t, courses.TryGetValue(t.CourseId, out var n) ? n : ""));
        }

        public async Task<IEnumerable<CurriculumTemplateDto>> GetByCourseAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            var templates = await _unitOfWork.CurriculumTemplates.GetByCourseAsync(courseId);
            return templates.Select(t => MapToDto(t, course?.Name ?? ""));
        }

        public async Task<CurriculumTemplateDto?> GetActiveByCourseAsync(int courseId)
        {
            var t = await _unitOfWork.CurriculumTemplates.GetActiveByCourseAsync(courseId);
            if (t == null) return null;
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            return MapToDto(t, course?.Name ?? "");
        }

        public async Task<CurriculumTemplateDetailDto?> GetDetailAsync(int id)
        {
            var t = await _unitOfWork.CurriculumTemplates.GetWithItemsAsync(id);
            if (t == null) return null;
            var course = await _unitOfWork.Courses.GetByIdAsync(t.CourseId);
            return new CurriculumTemplateDetailDto
            {
                Id = t.Id, CourseId = t.CourseId, CourseName = course?.Name ?? "",
                Name = t.Name, Description = t.Description, IsActive = t.IsActive,
                Items = t.Items.OrderBy(i => i.SessionNumber).Select(MapItem).ToList()
            };
        }

        public async Task<CurriculumTemplateDto> CreateAsync(CreateCurriculumTemplateDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId)
                ?? throw new NotFoundException("Kurs tapılmadı");

            // Yeni şablon aktiv olur — kursun digər aktiv şablonlarını deaktiv et
            await DeactivateOthersAsync(dto.CourseId, 0);

            var template = new CurriculumTemplate
            {
                CourseId = dto.CourseId,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CurriculumTemplates.AddAsync(template);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Create", "CurriculumTemplate", template.Id.ToString(),
                $"Şablon yaradıldı: {template.Name} ({course.Name})");

            return MapToDto(template, course.Name);
        }

        public async Task UpdateAsync(UpdateCurriculumTemplateDto dto)
        {
            var template = await _unitOfWork.CurriculumTemplates.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Şablon tapılmadı");
            template.Name = dto.Name;
            template.Description = dto.Description;
            _unitOfWork.CurriculumTemplates.Update(template);
            await _unitOfWork.SaveAsync();
        }

        public async Task SetActiveAsync(int id, bool active)
        {
            var template = await _unitOfWork.CurriculumTemplates.GetByIdAsync(id)
                ?? throw new NotFoundException("Şablon tapılmadı");

            if (active)
                await DeactivateOthersAsync(template.CourseId, id);

            template.IsActive = active;
            _unitOfWork.CurriculumTemplates.Update(template);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var template = await _unitOfWork.CurriculumTemplates.GetByIdAsync(id)
                ?? throw new NotFoundException("Şablon tapılmadı");
            _unitOfWork.CurriculumTemplates.SoftDelete(template);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Delete", "CurriculumTemplate", id.ToString(), $"Şablon silindi: {template.Name}");
        }

        // ── Items ──
        public async Task<int> GetNextSessionNumberAsync(int templateId)
        {
            var template = await _unitOfWork.CurriculumTemplates.GetWithItemsAsync(templateId)
                ?? throw new NotFoundException("Şablon tapılmadı");

            var items = template.Items.Where(i => !i.IsDeleted).ToList();
            if (items.Count == 0) return 1;
            return items.Max(i => i.SessionNumber) + 1;
        }

        public async Task<CurriculumTemplateItemDto> AddItemAsync(CreateTemplateItemDto dto)
        {
            var template = await _unitOfWork.CurriculumTemplates.GetWithItemsAsync(dto.CurriculumTemplateId)
                ?? throw new NotFoundException("Şablon tapılmadı");

            ValidateSession(dto.SessionNumber, dto.Topic, dto.HasAssessment, dto.AssessmentType, dto.AssessmentTitle);

            // Eyni şablonda eyni SessionNumber-ın olub-olmadığını yoxla (silinməyənlər arasında)
            if (template.Items.Any(i => !i.IsDeleted && i.SessionNumber == dto.SessionNumber))
                throw new ConflictException("Bu dərs nömrəsi artıq mövcuddur.");

            var item = new CurriculumTemplateItem
            {
                CurriculumTemplateId = dto.CurriculumTemplateId,
                SessionNumber = dto.SessionNumber,
                Topic = dto.Topic.Trim(),
                HasAssessment = dto.HasAssessment,
                AssessmentTitle = dto.HasAssessment ? dto.AssessmentTitle?.Trim() : null,
                AssessmentType = dto.HasAssessment ? dto.AssessmentType : null,
                WeightPercent = dto.HasAssessment ? dto.WeightPercent : null,
                MaxScore = dto.HasAssessment ? dto.MaxScore : null,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CurriculumTemplateItems.AddAsync(item);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Create", "CurriculumTemplateSession", item.Id.ToString(),
                $"Dərs sırası əlavə edildi: №{item.SessionNumber} — {item.Topic}");
            if (item.HasAssessment)
                await _audit.LogAsync("Create", "CurriculumTemplateAssessment", item.Id.ToString(),
                    $"Qiymətləndirmə əlavə edildi: №{item.SessionNumber} — {item.AssessmentTitle}");

            return MapItem(item);
        }

        public async Task<CurriculumTemplateItemDto> UpdateItemAsync(UpdateTemplateItemDto dto)
        {
            var item = await _unitOfWork.CurriculumTemplateItems.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Sıra tapılmadı");

            if (item.IsDeleted)
                throw new NotFoundException("Sıra tapılmadı");

            ValidateSession(dto.SessionNumber, dto.Topic, dto.HasAssessment, dto.AssessmentType, dto.AssessmentTitle);

            // SessionNumber dəyişibsə, dublikat yoxla (özündən başqa)
            if (item.SessionNumber != dto.SessionNumber)
            {
                var template = await _unitOfWork.CurriculumTemplates.GetWithItemsAsync(item.CurriculumTemplateId);
                if (template != null && template.Items.Any(x => !x.IsDeleted && x.Id != item.Id && x.SessionNumber == dto.SessionNumber))
                    throw new ConflictException("Bu dərs nömrəsi artıq mövcuddur.");
            }

            var hadAssessment = item.HasAssessment;

            item.SessionNumber  = dto.SessionNumber;
            item.Topic          = dto.Topic.Trim();
            item.HasAssessment  = dto.HasAssessment;
            item.AssessmentTitle = dto.HasAssessment ? dto.AssessmentTitle?.Trim() : null;
            item.AssessmentType  = dto.HasAssessment ? dto.AssessmentType : null;
            item.WeightPercent   = dto.HasAssessment ? dto.WeightPercent : null;
            item.MaxScore        = dto.HasAssessment ? dto.MaxScore : null;
            item.UpdatedAt       = DateTime.UtcNow;

            _unitOfWork.CurriculumTemplateItems.Update(item);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Update", "CurriculumTemplateSession", item.Id.ToString(),
                $"Dərs sırası yeniləndi: №{item.SessionNumber} — {item.Topic}");

            if (!hadAssessment && item.HasAssessment)
                await _audit.LogAsync("Create", "CurriculumTemplateAssessment", item.Id.ToString(),
                    $"Qiymətləndirmə əlavə edildi: №{item.SessionNumber} — {item.AssessmentTitle}");
            else if (hadAssessment && !item.HasAssessment)
                await _audit.LogAsync("Delete", "CurriculumTemplateAssessment", item.Id.ToString(),
                    $"Qiymətləndirmə silindi: №{item.SessionNumber}");
            else if (hadAssessment && item.HasAssessment)
                await _audit.LogAsync("Update", "CurriculumTemplateAssessment", item.Id.ToString(),
                    $"Qiymətləndirmə yeniləndi: №{item.SessionNumber} — {item.AssessmentTitle}");

            return MapItem(item);
        }

        public async Task DeleteItemAsync(int itemId)
        {
            var item = await _unitOfWork.CurriculumTemplateItems.GetByIdAsync(itemId)
                ?? throw new NotFoundException("Sıra tapılmadı");

            // Bu modeldə item özü həm sessionu həm assessment-i daşıyır,
            // ona görə item soft-delete olunanda assessment də avtomatik aradan qalxır.
            _unitOfWork.CurriculumTemplateItems.SoftDelete(item);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Delete", "CurriculumTemplateSession", item.Id.ToString(),
                $"Dərs sırası silindi: №{item.SessionNumber} — {item.Topic}");
            if (item.HasAssessment)
                await _audit.LogAsync("Delete", "CurriculumTemplateAssessment", item.Id.ToString(),
                    $"Bağlı qiymətləndirmə də silindi: {item.AssessmentTitle}");
        }

        public async Task ApplyToGroupAsync(int groupId, int templateId)
        {
            var template = await _unitOfWork.CurriculumTemplates.GetWithItemsAsync(templateId);
            if (template == null) return;

            var sessions = (await _unitOfWork.ClassSessions.GetByGroupAsync(groupId)).ToList();
            if (sessions.Count == 0) return;

            var now = DateTime.UtcNow;
            foreach (var item in template.Items.Where(i => !i.IsDeleted))
            {
                var session = sessions.FirstOrDefault(s => s.SessionNumber == item.SessionNumber);
                if (session == null) continue;

                // Mövzunu yaz
                session.Topic = item.Topic;
                _unitOfWork.ClassSessions.Update(session);

                // Qiymətləndirməni yarat (dublikatın qarşısı)
                if (item.HasAssessment && item.AssessmentType.HasValue)
                {
                    var exists = await _unitOfWork.AssessmentItems.AnyAsync(a => a.ClassSessionId == session.Id);
                    if (!exists)
                    {
                        await _unitOfWork.AssessmentItems.AddAsync(new AssessmentItem
                        {
                            GroupId = groupId,
                            Type = item.AssessmentType.Value,
                            Title = item.AssessmentTitle ?? item.Topic,
                            Date = session.Date.ToDateTime(session.StartTime),
                            MaxScore = item.MaxScore ?? 100m,
                            WeightPercent = item.WeightPercent ?? 0m,
                            ClassSessionId = session.Id,
                            IsVisible = true,
                            CreatedAt = now
                        });
                    }
                }
            }
            await _unitOfWork.SaveAsync();
        }

        // ── Helpers ──
        private static void ValidateSession(int sessionNumber, string topic,
            bool hasAssessment, Entities.Enums.AssessmentType? type, string? title)
        {
            if (sessionNumber < 1)
                throw new BadRequestException("Dərs nömrəsi 1-dən kiçik ola bilməz");
            if (string.IsNullOrWhiteSpace(topic))
                throw new BadRequestException("Mövzu tələb olunur");
            if (hasAssessment && (type == null || string.IsNullOrWhiteSpace(title)))
                throw new BadRequestException("Qiymətləndirmə üçün başlıq və növ tələb olunur");
        }

        private async Task DeactivateOthersAsync(int courseId, int exceptId)
        {
            var others = await _unitOfWork.CurriculumTemplates.GetByCourseAsync(courseId);
            foreach (var o in others.Where(o => o.IsActive && o.Id != exceptId))
            {
                o.IsActive = false;
                _unitOfWork.CurriculumTemplates.Update(o);
            }
        }

        private static CurriculumTemplateDto MapToDto(CurriculumTemplate t, string courseName) => new()
        {
            Id = t.Id, CourseId = t.CourseId, CourseName = courseName,
            Name = t.Name, Description = t.Description, IsActive = t.IsActive,
            ItemCount = t.Items?.Count(i => !i.IsDeleted) ?? 0,
            AssessmentCount = t.Items?.Count(i => !i.IsDeleted && i.HasAssessment) ?? 0
        };

        private static CurriculumTemplateItemDto MapItem(CurriculumTemplateItem i) => new()
        {
            Id = i.Id, SessionNumber = i.SessionNumber, Topic = i.Topic,
            HasAssessment = i.HasAssessment, AssessmentTitle = i.AssessmentTitle,
            AssessmentType = i.AssessmentType, WeightPercent = i.WeightPercent, MaxScore = i.MaxScore
        };
    }
}
