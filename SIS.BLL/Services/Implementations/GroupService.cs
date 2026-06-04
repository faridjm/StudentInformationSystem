using SIS.BLL.Services.DTOs.Groups;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _audit;
        private readonly ICurriculumTemplateService _curriculum;

        public GroupService(IUnitOfWork unitOfWork, IAuditLogService audit, ICurriculumTemplateService curriculum)
        {
            _unitOfWork = unitOfWork;
            _audit = audit;
            _curriculum = curriculum;
        }

        public async Task<IEnumerable<GroupListDto>> GetAllAsync()
        {
            var groups = await _unitOfWork.Groups.GetActiveGroupsAsync();
            return groups.Select(MapToListDto);
        }

        public async Task<GroupDetailDto?> GetByIdAsync(int id)
        {
            var g = await _unitOfWork.Groups.GetGroupWithDetailsAsync(id);
            return g == null ? null : MapToDetailDto(g);
        }

        public async Task<IEnumerable<GroupListDto>> GetByTeacherAsync(int teacherId)
        {
            var groups = await _unitOfWork.Groups.GetByTeacherAsync(teacherId);
            return groups.Select(MapToListDto);
        }

        public async Task<GroupDetailDto> CreateAsync(CreateGroupDto dto)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new BadRequestException("Başlama tarixi bitmə tarixindən əvvəl olmalıdır");

            if (dto.AcademicYearId.HasValue)
            {
                var season = await _unitOfWork.AcademicYears.GetByIdAsync(dto.AcademicYearId.Value);
                if (season != null)
                {
                    if (dto.StartDate < season.StartDate)
                        throw new BadRequestException($"Qrupun başlama tarixi sezondan ({season.StartDate:dd.MM.yyyy}) əvvəl ola bilməz");
                    if (dto.EndDate > season.EndDate)
                        throw new BadRequestException($"Qrupun bitmə tarixi sezondan ({season.EndDate:dd.MM.yyyy}) gec ola bilməz");
                }
            }

            var group = new Group
            {
                Name = dto.Name,
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId,
                AcademicYearId = dto.AcademicYearId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Capacity = dto.Capacity,
                CreatedAt = DateTime.UtcNow,
                ScheduleDays = dto.ScheduleDays.Select(s => new GroupScheduleDay
                {
                    DayOfWeek = s.DayOfWeek,
                    StartTime = TimeOnly.Parse(s.StartTime),
                    EndTime = TimeOnly.Parse(s.EndTime),
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };

            await _unitOfWork.Groups.AddAsync(group);
            await _unitOfWork.SaveAsync();

            await GenerateSessionsAsync(group.Id);

            // Seçilmiş curriculum şablonunu tətbiq et (mövzular + qiymətləndirmələr)
            if (dto.CurriculumTemplateId.HasValue)
                await _curriculum.ApplyToGroupAsync(group.Id, dto.CurriculumTemplateId.Value);

            await _audit.LogAsync("Create", "Group", group.Id.ToString(), $"Qrup yaradıldı: {group.Name}");

            var created = await _unitOfWork.Groups.GetGroupWithDetailsAsync(group.Id);
            return MapToDetailDto(created!);
        }

        public async Task UpdateAsync(UpdateGroupDto dto)
        {
            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(dto.Id)
                ?? throw new NotFoundException("Qrup tapılmadı");

            if (dto.StartDate >= dto.EndDate)
                throw new BadRequestException("Başlama tarixi bitmə tarixindən əvvəl olmalıdır");

            if (dto.AcademicYearId.HasValue)
            {
                var season = await _unitOfWork.AcademicYears.GetByIdAsync(dto.AcademicYearId.Value);
                if (season != null)
                {
                    if (dto.StartDate < season.StartDate)
                        throw new BadRequestException($"Qrupun başlama tarixi sezondan ({season.StartDate:dd.MM.yyyy}) əvvəl ola bilməz");
                    if (dto.EndDate > season.EndDate)
                        throw new BadRequestException($"Qrupun bitmə tarixi sezondan ({season.EndDate:dd.MM.yyyy}) gec ola bilməz");
                }
            }

            group.Name = dto.Name;
            group.TeacherId = dto.TeacherId;
            group.AcademicYearId = dto.AcademicYearId;
            group.StartDate = dto.StartDate;
            group.EndDate = dto.EndDate;
            group.Capacity = dto.Capacity;
            group.IsActive = dto.IsActive;

            // Cədvəl günlərini yenilə
            group.ScheduleDays.Clear();
            foreach (var s in dto.ScheduleDays)
                group.ScheduleDays.Add(new GroupScheduleDay
                {
                    GroupId = group.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = TimeOnly.Parse(s.StartTime),
                    EndTime = TimeOnly.Parse(s.EndTime),
                    CreatedAt = DateTime.UtcNow
                });

            _unitOfWork.Groups.Update(group);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Update", "Group", group.Id.ToString(), $"Qrup yeniləndi: {group.Name}");
        }

        public async Task DeleteAsync(int id)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(id)
                ?? throw new NotFoundException("Qrup tapılmadı");
            _unitOfWork.Groups.SoftDelete(group);
            await _unitOfWork.SaveAsync();
        }

        public async Task GenerateSessionsAsync(int groupId)
        {
            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(groupId)
                ?? throw new NotFoundException("Qrup tapılmadı");

            var existing = await _unitOfWork.ClassSessions.GetByGroupAsync(groupId);
            foreach (var s in existing)
                _unitOfWork.ClassSessions.SoftDelete(s);

            var sessions = new List<ClassSession>();
            var current = DateOnly.FromDateTime(group.StartDate);
            var end = DateOnly.FromDateTime(group.EndDate);
            int sessionNumber = 1;

            while (current <= end)
            {
                foreach (var schedDay in group.ScheduleDays.OrderBy(d => d.DayOfWeek))
                {
                    if (schedDay.DayOfWeek == current.DayOfWeek)
                    {
                        sessions.Add(new ClassSession
                        {
                            GroupId = groupId,
                            Date = current,
                            StartTime = schedDay.StartTime,
                            EndTime = schedDay.EndTime,
                            SessionNumber = sessionNumber++,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                current = current.AddDays(1);
            }

            foreach (var s in sessions)
                await _unitOfWork.ClassSessions.AddAsync(s);

            await _unitOfWork.SaveAsync();
        }

        private static GroupListDto MapToListDto(Group g) => new()
        {
            Id = g.Id, Name = g.Name,
            CourseName = g.Course?.Name ?? "",
            TeacherName = g.Teacher?.AppUser?.FullName ?? "",
            SeasonName = g.AcademicYear?.Name,
            StartDate = g.StartDate, EndDate = g.EndDate,
            Capacity = g.Capacity,
            EnrolledCount = g.Enrollments?.Count(e => !e.IsDeleted && e.Student != null) ?? 0,
            IsActive = g.IsActive
        };

        private static GroupDetailDto MapToDetailDto(Group g) => new()
        {
            Id = g.Id, Name = g.Name,
            CourseId = g.CourseId, TeacherId = g.TeacherId,
            CourseName = g.Course?.Name ?? "",
            TeacherName = g.Teacher?.AppUser?.FullName ?? "",
            SeasonName = g.AcademicYear?.Name,
            StartDate = g.StartDate, EndDate = g.EndDate,
            Capacity = g.Capacity,
            EnrolledCount = g.Enrollments?.Count(e => !e.IsDeleted && e.Student != null) ?? 0,
            IsActive = g.IsActive,
            ScheduleDays = g.ScheduleDays?.Select(s => new ScheduleDayDto
            {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm")
            }).ToList() ?? new()
        };
    }
}
