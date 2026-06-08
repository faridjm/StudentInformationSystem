using SIS.BLL.Services.DTOs.Groups;
using SIS.BLL.Services.DTOs.Performance;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _audit;
        private readonly ICurriculumTemplateService _curriculum;
        private readonly ICurriculumScheduleCalculatorService _scheduleCalculator;
        private readonly IRoomService _roomService;
        private readonly IAcademicPerformanceService _performance;

        public GroupService(IUnitOfWork unitOfWork, IAuditLogService audit, ICurriculumTemplateService curriculum,
            ICurriculumScheduleCalculatorService scheduleCalculator, IRoomService roomService,
            IAcademicPerformanceService performance)
        {
            _unitOfWork = unitOfWork;
            _audit = audit;
            _curriculum = curriculum;
            _scheduleCalculator = scheduleCalculator;
            _roomService = roomService;
            _performance = performance;
        }

        /// <summary>
        /// Otaq seçimini doğrular: Online qruplar otaq saxlamır; seçilmiş otaq aktiv olmalıdır;
        /// eyni vaxtda başqa qrupla konflikt olmamalıdır. Konflikt olarsa bloklayır.
        /// Calculation/scheduling məntiqini dəyişmir — yalnız otaq təyinatını yoxlayır.
        /// </summary>
        private async Task<int?> ResolveAndValidateRoomAsync(
            int? roomId, DeliveryMode mode, List<CreateScheduleDayDto> scheduleDays,
            DateTime start, DateTime end, int? excludeGroupId, string groupName)
        {
            // Online qrup fiziki otaq saxlamır
            if (mode == DeliveryMode.Online)
                return null;

            if (!roomId.HasValue)
                return null;

            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId.Value)
                ?? throw new BadRequestException("Seçilmiş otaq tapılmadı");

            if (!room.IsActive)
                throw new BadRequestException("Seçilmiş otaq aktiv deyil və qrupa təyin edilə bilməz");

            var slots = scheduleDays.Select(s => new SIS.BLL.Services.DTOs.Room.ScheduleSlotInput
            {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList();

            var conflict = await _roomService.CheckRoomConflictAsync(roomId.Value, slots, start, end, excludeGroupId);
            if (conflict.HasConflict)
            {
                await _audit.LogAsync("RoomScheduleConflictBlocked", "Room", roomId.Value.ToString(),
                    $"Room conflict blocked: {room.Name} already occupied {conflict.DayOfWeek} {conflict.StartTime}–{conflict.EndTime}.");
                throw new ConflictException(conflict.Message ?? "Bu otaq seçilmiş dərs vaxtında artıq başqa qrup üçün ayrılıb.");
            }

            return roomId.Value;
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
            // EndDate-i server tərəfdə yenidən hesabla — frontend-ə güvənmə.
            // Tədris şablonu və dərs günləri varsa, bitmə tarixi şablondakı dərs sayına görə təyin edilir.
            if (dto.CurriculumTemplateId.HasValue && dto.ScheduleDays.Any())
            {
                var calc = await _scheduleCalculator.CalculateAsync(new CurriculumScheduleRequest
                {
                    StartDate = dto.StartDate,
                    ScheduleDays = dto.ScheduleDays.Select(s => s.DayOfWeek).ToList(),
                    CurriculumTemplateId = dto.CurriculumTemplateId
                });

                if (calc.Success && calc.EndDate.HasValue)
                    dto.EndDate = calc.EndDate.Value;
            }

            if (dto.StartDate >= dto.EndDate)
                throw new BadRequestException("Başlama tarixi bitmə tarixindən əvvəl olmalıdır");

            // Tədris formatı yoxlaması
            if (!Enum.IsDefined(typeof(DeliveryMode), dto.DeliveryMode))
                throw new BadRequestException("Tədris formatı (DeliveryMode) düzgün deyil");

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

            // Otaq seçimi və conflict yoxlaması (InPerson üçün create-də otaq tələb olunur)
            if (dto.DeliveryMode == DeliveryMode.InPerson && !dto.RoomId.HasValue)
                throw new BadRequestException("Əyani (InPerson) qrup üçün otaq seçilməlidir");

            var resolvedRoomId = await ResolveAndValidateRoomAsync(
                dto.RoomId, dto.DeliveryMode, dto.ScheduleDays, dto.StartDate, dto.EndDate, null, dto.Name);

            var group = new Group
            {
                Name = dto.Name,
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId,
                AcademicYearId = dto.AcademicYearId,
                RoomId = resolvedRoomId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Capacity = dto.Capacity,
                DeliveryMode = dto.DeliveryMode,
                DefaultOnlinePlatform = dto.DeliveryMode == DeliveryMode.InPerson ? null : dto.DefaultOnlinePlatform,
                DefaultMeetingUrl = dto.DeliveryMode == DeliveryMode.InPerson ? null : dto.DefaultMeetingUrl,
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

            await _audit.LogAsync("Create", "Group", group.Id.ToString(), $"Qrup yaradıldı: {group.Name} (Tədris formatı: {group.DeliveryMode})");

            if (resolvedRoomId.HasValue)
                await _audit.LogAsync("AssignRoomToGroup", "Group", group.Id.ToString(),
                    $"Room {resolvedRoomId} assigned to group {group.Name}.");

            var created = await _unitOfWork.Groups.GetGroupWithDetailsAsync(group.Id);
            return MapToDetailDto(created!);
        }

        public async Task UpdateAsync(UpdateGroupDto dto)
        {
            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(dto.Id)
                ?? throw new NotFoundException("Qrup tapılmadı");

            // ── Cədvəl dəyişikliyini aşkar et (regenerate sessions zərurəti üçün) ──
            // İstifadəçi dərs sayını verirsə, EndDate-i avtomatik hesabla.
            if (dto.RegenerateSessions && dto.TotalSessions.HasValue && dto.TotalSessions.Value > 0
                && dto.ScheduleDays.Count > 0)
            {
                var calc = _scheduleCalculator.CalculateEndDate(
                    dto.StartDate,
                    dto.ScheduleDays.Select(s => s.DayOfWeek),
                    dto.TotalSessions.Value);
                dto.EndDate = calc;
            }

            if (dto.StartDate >= dto.EndDate)
                throw new BadRequestException("Başlama tarixi bitmə tarixindən əvvəl olmalıdır");

            // Tədris formatı yoxlaması
            if (!Enum.IsDefined(typeof(DeliveryMode), dto.DeliveryMode))
                throw new BadRequestException("Tədris formatı (DeliveryMode) düzgün deyil");

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

            // Schedule diff (regenerate qərarı üçün)
            bool scheduleChanged = HasScheduleChanged(group, dto);

            // Otaq seçimi və conflict yoxlaması (öz cədvəlini conflict saymamaq üçün excludeGroupId)
            var previousRoomId = group.RoomId;
            var resolvedRoomId = await ResolveAndValidateRoomAsync(
                dto.RoomId, dto.DeliveryMode, dto.ScheduleDays, dto.StartDate, dto.EndDate, dto.Id, dto.Name);

            group.Name = dto.Name;
            group.TeacherId = dto.TeacherId;
            group.AcademicYearId = dto.AcademicYearId;
            group.RoomId = resolvedRoomId;
            group.StartDate = dto.StartDate;
            group.EndDate = dto.EndDate;
            group.Capacity = dto.Capacity;
            group.IsActive = dto.IsActive;
            group.DeliveryMode = dto.DeliveryMode;
            group.DefaultOnlinePlatform = dto.DeliveryMode == DeliveryMode.InPerson ? null : dto.DefaultOnlinePlatform;
            group.DefaultMeetingUrl = dto.DeliveryMode == DeliveryMode.InPerson ? null : dto.DefaultMeetingUrl;

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

            // Cədvəl dəyişib və admin regenerate seçibsə, dərs sessiyalarını yenidən yarat.
            if (dto.RegenerateSessions && scheduleChanged)
            {
                // Köhnə sessions-ı (soft-delete-dən əvvəl) yadda saxla — assessment sync üçün
                var oldSessions = (await _unitOfWork.ClassSessions.GetByGroupAsync(group.Id)).ToList();

                await GenerateSessionsAsync(group.Id);

                // Sonuncu session-dan EndDate-i avtomatik təyin et
                var newSessions = (await _unitOfWork.ClassSessions.GetByGroupAsync(group.Id)).ToList();
                var activeNew = newSessions.Where(s => !s.IsDeleted).ToList();
                var lastDate = activeNew
                    .OrderByDescending(s => s.Date).ThenByDescending(s => s.StartTime)
                    .FirstOrDefault()?.Date;
                if (lastDate.HasValue)
                {
                    group.EndDate = lastDate.Value.ToDateTime(TimeOnly.MinValue);
                    _unitOfWork.Groups.Update(group);
                    await _unitOfWork.SaveAsync();
                }

                // Assessment plan sync — köhnə ClassSession.SessionNumber → yeni ClassSession date
                await SyncAssessmentDatesAsync(group.Id, oldSessions, activeNew);

                await _audit.LogAsync("RegenerateGroupSessions", "Group", group.Id.ToString(),
                    $"Qrupun dərs cədvəli yenidən yaradıldı: {group.Name}");
            }

            await _audit.LogAsync("Update", "Group", group.Id.ToString(), $"Qrup yeniləndi: {group.Name}");

            if (previousRoomId != resolvedRoomId)
                await _audit.LogAsync("ChangeGroupRoom", "Group", group.Id.ToString(),
                    $"Group {group.Name} room changed from {previousRoomId?.ToString() ?? "—"} to {resolvedRoomId?.ToString() ?? "—"}.");
        }

        /// <summary>
        /// Mövcud qrupun cədvəli ilə yeni dto cədvəli arasında fərq varmı?
        /// StartDate, EndDate, ScheduleDays (gün+saat) müqayisə olunur.
        /// </summary>
        private static bool HasScheduleChanged(Group group, UpdateGroupDto dto)
        {
            if (group.StartDate.Date != dto.StartDate.Date) return true;
            if (group.EndDate.Date != dto.EndDate.Date) return true;

            var existing = group.ScheduleDays
                .Select(s => (Day: s.DayOfWeek, Start: s.StartTime, End: s.EndTime))
                .OrderBy(x => x.Day).ThenBy(x => x.Start)
                .ToList();

            var incoming = dto.ScheduleDays
                .Select(s => (Day: s.DayOfWeek, Start: TimeOnly.Parse(s.StartTime), End: TimeOnly.Parse(s.EndTime)))
                .OrderBy(x => x.Day).ThenBy(x => x.Start)
                .ToList();

            if (existing.Count != incoming.Count) return true;
            for (int i = 0; i < existing.Count; i++)
            {
                if (existing[i].Day != incoming[i].Day) return true;
                if (existing[i].Start != incoming[i].Start) return true;
                if (existing[i].End != incoming[i].End) return true;
            }
            return false;
        }

        /// <summary>
        /// Schedule regenerate-dən sonra qrupun assessment plan tarixlərini yeni ClassSession
        /// tarixləri ilə sinxronlaşdırır. Assessment-in Submissions/qiymətləri toxunulmur — yalnız
        /// Date və ClassSessionId yenilənir.
        ///
        /// Məntiq: hər assessment köhnə ClassSessionId vasitəsilə bir SessionNumber-ə bağlıdır.
        /// Yeni siyahıda eyni SessionNumber-ə uyğun sessiyanı tapırıq və assessment-i həmin sessiyaya köçürürük.
        /// SessionNumber tapılmırsa (manual əlavə olunmuş və ya bağlantısız assessment), toxunulmur.
        /// </summary>
        private async Task SyncAssessmentDatesAsync(int groupId, List<ClassSession> oldSessions, List<ClassSession> newSessions)
        {
            // Köhnə sessionId → SessionNumber map-i (silinmiş və silinməmiş hamısı)
            var oldIdToNumber = oldSessions.ToDictionary(s => s.Id, s => s.SessionNumber);
            // Yeni SessionNumber → ClassSession map-i
            var newByNumber = newSessions.ToDictionary(s => s.SessionNumber, s => s);

            var assessments = (await _unitOfWork.AssessmentItems.GetByGroupAsync(groupId)).ToList();
            int updated = 0, skipped = 0;

            foreach (var a in assessments)
            {
                if (a.IsDeleted) continue;
                if (!a.ClassSessionId.HasValue)
                {
                    // Manual əlavə olunmuş və ya köhnə assessment — toxunma
                    skipped++;
                    continue;
                }

                if (!oldIdToNumber.TryGetValue(a.ClassSessionId.Value, out var sessionNumber))
                {
                    // Köhnə sessiya tapılmadı (məsələn artıq əvvəl silinib) — toxunma
                    skipped++;
                    continue;
                }

                if (!newByNumber.TryGetValue(sessionNumber, out var newSession))
                {
                    // Yeni cədvəldə bu SessionNumber yoxdur (məs: dərs sayı azalıb) — toxunma
                    skipped++;
                    continue;
                }

                // Yeni sessiyaya keçir
                a.ClassSessionId = newSession.Id;
                a.Date = newSession.Date.ToDateTime(newSession.StartTime);
                a.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.AssessmentItems.Update(a);
                updated++;
            }

            if (updated > 0 || skipped > 0)
            {
                await _unitOfWork.SaveAsync();
                await _audit.LogAsync("SyncAssessmentDates", "Group", groupId.ToString(),
                    $"Qiymətləndirmə tarixləri yeni cədvələ uyğun yeniləndi: {updated} yeniləndi, {skipped} toxunulmadı.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(id)
                ?? throw new NotFoundException("Qrup tapılmadı");
            _unitOfWork.Groups.SoftDelete(group);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<GroupStudentPerformanceDto>> GetGroupPerformanceAsync(int groupId)
        {
            var enrollments = (await _unitOfWork.Enrollments.GetByGroupAsync(groupId)).ToList();
            var result = new List<GroupStudentPerformanceDto>(enrollments.Count);

            foreach (var e in enrollments)
            {
                if (e.IsDeleted) continue;
                AcademicPerformanceDto perf;
                try { perf = await _performance.GetEnrollmentPerformanceAsync(e.Id); }
                catch
                {
                    // hesablama uğursuz olarsa boş şablon ver
                    perf = new AcademicPerformanceDto
                    {
                        EnrollmentId = e.Id, StudentId = e.StudentId, GroupId = groupId,
                        LetterGrade = "—", PerformanceStatus = ""
                    };
                }

                result.Add(new GroupStudentPerformanceDto
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student?.AppUser?.FullName ?? "",
                    StudentNumber = e.Student?.StudentNumber ?? "",
                    StudentEmail = e.Student?.AppUser?.Email,
                    TotalCompletedSessions = perf.TotalCompletedSessions,
                    AttendanceTakenSessions = perf.AttendanceTakenSessions,
                    PresentCount = perf.PresentCount,
                    LateCount = perf.LateCount,
                    AbsentCount = perf.AbsentCount,
                    ExcusedCount = perf.ExcusedCount,
                    AttendancePercentage = perf.AttendancePercentage,
                    OfficialGrade = perf.OfficialGrade,
                    CompletedWeight = perf.CompletedWeight,
                    TotalWeight = perf.TotalWeight,
                    LetterGrade = perf.LetterGrade,
                    GPA = perf.GPA,
                    PerformanceStatus = perf.PerformanceStatus,
                    CertificateEligible = perf.CertificateEligible
                });
            }

            return result.OrderBy(r => r.StudentName).ToList();
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
            IsActive = g.IsActive,
            DeliveryMode = g.DeliveryMode,
            DefaultOnlinePlatform = g.DefaultOnlinePlatform,
            DefaultMeetingUrl = g.DefaultMeetingUrl,
            RoomId = g.RoomId,
            RoomName = g.Room?.Name
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
            DeliveryMode = g.DeliveryMode,
            DefaultOnlinePlatform = g.DefaultOnlinePlatform,
            DefaultMeetingUrl = g.DefaultMeetingUrl,
            RoomId = g.RoomId,
            RoomName = g.Room?.Name,
            ScheduleDays = g.ScheduleDays?.Select(s => new ScheduleDayDto
            {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm")
            }).ToList() ?? new()
        };
    }
}
