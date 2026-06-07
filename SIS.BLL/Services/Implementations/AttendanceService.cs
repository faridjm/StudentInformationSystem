using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly IAuditLogService _audit;
        private readonly IAcademicPerformanceService _performance;

        public AttendanceService(IUnitOfWork unitOfWork, IClock clock, IAuditLogService audit,
            IAcademicPerformanceService performance)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _audit = audit;
            _performance = performance;
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByEnrollmentAsync(int enrollmentId)
        {
            var records = await _unitOfWork.AttendanceRecords.GetAttendanceByEnrollmentAsync(enrollmentId);
            return records.Select(MapToDto);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceBySessionAsync(int sessionId)
        {
            var records = await _unitOfWork.AttendanceRecords.GetAttendanceBySessionAsync(sessionId);
            return records.Select(MapToDto);
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int enrollmentId)
            => await _performance.GetAttendanceSummaryAsync(enrollmentId);

        public async Task RecordBulkAttendanceAsync(IEnumerable<CreateAttendanceDto> dtos)
        {
            var dtoList = dtos.ToList();
            if (dtoList.Count == 0) return;

            int? sessionId = dtoList[0].ClassSessionId;
            ClassSession? session = null;

            if (sessionId.HasValue)
            {
                session = await _unitOfWork.ClassSessions.GetByIdAsync(sessionId.Value)
                    ?? throw new NotFoundException("Dərs sessiyası tapılmadı");

                if (session.Status == ClassSessionStatus.Cancelled)
                    throw new BadRequestException("Ləğv olunmuş sessiya üçün davamiyyət qeyd edilə bilməz");

                foreach (var d in dtoList)
                    d.SessionDate = session.Date.ToDateTime(TimeOnly.MinValue);
            }

            foreach (var dto in dtoList)
            {
                AttendanceRecord? existing = null;
                if (dto.ClassSessionId.HasValue)
                    existing = await _unitOfWork.AttendanceRecords.GetAsync(
                        a => a.EnrollmentId == dto.EnrollmentId && a.ClassSessionId == dto.ClassSessionId);
                else
                    existing = await _unitOfWork.AttendanceRecords.GetAsync(
                        a => a.EnrollmentId == dto.EnrollmentId && a.SessionDate.Date == dto.SessionDate.Date);

                if (existing != null)
                {
                    existing.Status = dto.Status;
                    existing.Note = dto.Note;
                    _unitOfWork.AttendanceRecords.Update(existing);
                }
                else
                {
                    await _unitOfWork.AttendanceRecords.AddAsync(new AttendanceRecord
                    {
                        EnrollmentId = dto.EnrollmentId,
                        ClassSessionId = dto.ClassSessionId,
                        SessionDate = dto.SessionDate,
                        Status = dto.Status,
                        Note = dto.Note,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Save", "Attendance", sessionId?.ToString(),
                $"Davamiyyət qeyd edildi: {dtoList.Count} tələbə" + (sessionId.HasValue ? $" (Session #{sessionId})" : ""));

            if (session != null && session.Status == ClassSessionStatus.Scheduled)
            {
                session.Status = ClassSessionStatus.Completed;
                session.UpdatedAt = _clock.UtcNow;
                _unitOfWork.ClassSessions.Update(session);
                await _unitOfWork.SaveAsync();
            }
        }

        private static AttendanceDto MapToDto(AttendanceRecord a) => new()
        {
            Id = a.Id, EnrollmentId = a.EnrollmentId,
            StudentName = a.Enrollment?.Student?.AppUser?.FullName ?? "",
            StudentNumber = a.Enrollment?.Student?.StudentNumber ?? "",
            SessionDate = a.SessionDate, Status = a.Status,
            Note = a.Note, ClassSessionId = a.ClassSessionId
        };

        // ── Demo Attendance Simulation (Teacher panel only) ──

        public async Task<DemoAttendanceResultDto> GenerateTeacherDemoAttendanceAsync(
            GenerateDemoAttendanceDto dto, string teacherUserId)
        {
            // 1) Sahiblik: müəllim öz qrupunda olmalıdır
            var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(teacherUserId)
                ?? throw new UnauthorizedException("Müəllim profili tapılmadı");

            var group = await _unitOfWork.Groups.GetByIdAsync(dto.GroupId)
                ?? throw new NotFoundException("Qrup tapılmadı");

            if (group.TeacherId != teacher.Id)
                throw new UnauthorizedException("Bu qrup üçün demo generasiya icazəniz yoxdur");

            if (dto.FromSessionNumber <= 0 || dto.ToSessionNumber < dto.FromSessionNumber)
                throw new BadRequestException("Sessiya aralığı düzgün deyil");

            // 2) Sessiyalar (cancelled istisna)
            var allSessions = (await _unitOfWork.ClassSessions.GetByGroupAsync(dto.GroupId))
                .Where(s => s.Status != ClassSessionStatus.Cancelled
                         && s.SessionNumber >= dto.FromSessionNumber
                         && s.SessionNumber <= dto.ToSessionNumber)
                .OrderBy(s => s.SessionNumber)
                .ToList();
            if (allSessions.Count == 0)
                throw new BadRequestException("Seçilmiş aralıqda dərs sessiyası tapılmadı");

            // CustomCounts validation
            if (dto.Mode == DemoAttendanceMode.CustomCounts)
            {
                var p = dto.PresentCount ?? 0;
                var a = dto.AbsentCount ?? 0;
                var l = dto.LateCount ?? 0;
                var ex = dto.ExcusedCount ?? 0;
                if (p < 0 || a < 0 || l < 0 || ex < 0)
                    throw new BadRequestException("Status sayları mənfi ola bilməz");
                if (p + a + l + ex != allSessions.Count)
                    throw new BadRequestException(
                        $"Status saylarının cəmi seçilmiş sessiya sayına bərabər olmalıdır ({allSessions.Count})");
            }

            // 3) Aktiv enrollment-lər
            var enrollments = (await _unitOfWork.Enrollments.GetByGroupAsync(dto.GroupId))
                .Where(e => !e.IsDeleted && e.Status == EnrollmentStatus.Active)
                .ToList();
            if (enrollments.Count == 0)
                throw new BadRequestException("Qrupda aktiv tələbə yoxdur");

            // 4) Hər tələbə üçün status ardıcıllığı qur
            var rng = new Random();
            // CustomCounts modunda eyni pattern bütün tələbələrə tətbiq olunur
            List<AttendanceStatus>? sharedCustomSeq = null;
            if (dto.Mode == DemoAttendanceMode.CustomCounts)
            {
                sharedCustomSeq = BuildCustomCountsSequence(allSessions.Count,
                    dto.PresentCount ?? 0, dto.AbsentCount ?? 0,
                    dto.LateCount ?? 0, dto.ExcusedCount ?? 0, rng);
            }

            int created = 0, updated = 0, skipped = 0;
            var now = _clock.UtcNow;

            // 5) Hər tələbə + hər sessiya cütü üçün status təyin et
            foreach (var enrollment in enrollments)
            {
                List<AttendanceStatus> statuses;
                if (dto.Mode == DemoAttendanceMode.CustomCounts)
                {
                    // Hər tələbə üçün eyni pattern (yenidən shuffle edilə bilər; saymaq dəyişmir)
                    statuses = sharedCustomSeq!.OrderBy(_ => rng.Next()).ToList();
                }
                else if (dto.Mode == DemoAttendanceMode.AllPresent)
                {
                    statuses = Enumerable.Repeat(AttendanceStatus.Present, allSessions.Count).ToList();
                }
                else
                {
                    var profile = PickRealisticTarget(enrollments.IndexOf(enrollment), enrollments.Count, rng);
                    statuses = BuildStatusSequence(allSessions.Count, profile, dto.Mode, rng);
                }

                for (int i = 0; i < allSessions.Count; i++)
                {
                    var session = allSessions[i];
                    var status = statuses[i];

                    var existing = await _unitOfWork.AttendanceRecords.GetAsync(
                        a => a.EnrollmentId == enrollment.Id && a.ClassSessionId == session.Id);

                    if (existing != null)
                    {
                        if (!dto.OverwriteExisting) { skipped++; continue; }
                        existing.Status = status;
                        existing.Note = "[Demo]";
                        _unitOfWork.AttendanceRecords.Update(existing);
                        updated++;
                    }
                    else
                    {
                        await _unitOfWork.AttendanceRecords.AddAsync(new AttendanceRecord
                        {
                            EnrollmentId = enrollment.Id,
                            ClassSessionId = session.Id,
                            SessionDate = session.Date.ToDateTime(TimeOnly.MinValue),
                            Status = status,
                            Note = "[Demo]",
                            CreatedAt = now
                        });
                        created++;
                    }
                }
            }

            await _unitOfWork.SaveAsync();

            var auditDesc = $"Müəllim {teacher.AppUser?.FullName ?? teacher.EmployeeNumber} qrupu '{group.Name}' üçün demo davamiyyət yaratdı (sessiyalar #{dto.FromSessionNumber}-#{dto.ToSessionNumber}, mode: {dto.Mode}). Created: {created}, Updated: {updated}, Skipped: {skipped}";
            if (dto.Mode == DemoAttendanceMode.CustomCounts)
                auditDesc += $". Pattern: {dto.PresentCount} Present, {dto.AbsentCount} Absent, {dto.LateCount} Late, {dto.ExcusedCount} Excused";
            await _audit.LogAsync("GenerateDemoAttendance", "Group", group.Id.ToString(), auditDesc);

            return new DemoAttendanceResultDto
            {
                CreatedCount = created,
                UpdatedCount = updated,
                SkippedCount = skipped,
                SessionsAffected = allSessions.Count,
                StudentsAffected = enrollments.Count,
                Message = $"Demo davamiyyət yaradıldı — {created} əlavə, {updated} yeniləmə, {skipped} keçildi"
            };
        }

        /// <summary>Realistic profillərə görə tələbə üçün hədəf attendance %.</summary>
        private static int PickRealisticTarget(int index, int total, Random rng)
        {
            // Tək tələbə qrupu — 80-95% arası
            if (total <= 1) return rng.Next(80, 96);
            // Təxminən: 50% tələbə yüksək (90-100), 35% orta (75-85), 15% risk (60-70)
            var bucket = (double)index / total;
            if (bucket < 0.50) return rng.Next(90, 101);
            if (bucket < 0.85) return rng.Next(75, 86);
            return rng.Next(60, 71);
        }

        /// <summary>Custom Counts üçün dəqiq say-əsaslı qarışıq ardıcıllıq qurur.</summary>
        private static List<AttendanceStatus> BuildCustomCountsSequence(int sessionCount,
            int present, int absent, int late, int excused, Random rng)
        {
            var seq = new List<AttendanceStatus>(sessionCount);
            for (int i = 0; i < present; i++) seq.Add(AttendanceStatus.Present);
            for (int i = 0; i < absent; i++) seq.Add(AttendanceStatus.Absent);
            for (int i = 0; i < late; i++) seq.Add(AttendanceStatus.Late);
            for (int i = 0; i < excused; i++) seq.Add(AttendanceStatus.Excused);
            // Real görünmək üçün qarışdır
            for (int i = seq.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (seq[i], seq[j]) = (seq[j], seq[i]);
            }
            return seq;
        }

        /// <summary>Hədəf faizə uyğun (Present/Late/Excused/Absent) ardıcıllığı qurur.</summary>
        private static List<AttendanceStatus> BuildStatusSequence(int sessionCount, int targetPct,
            DemoAttendanceMode mode, Random rng)
        {
            // Score: Present=1.0, Excused=1.0, Late=0.5, Absent=0
            // Yaxınlaşdırmaq üçün: present count = targetPct% sessions
            int presentLike = (int)Math.Round(sessionCount * (targetPct / 100.0));
            int absentLike = sessionCount - presentLike;

            // Realistic-də Late və Excused əlavə et
            int lateCount = 0, excusedCount = 0;
            if (mode == DemoAttendanceMode.RealisticDemo && presentLike > 2)
            {
                lateCount = Math.Min(2, rng.Next(0, presentLike / 4 + 1));
                excusedCount = Math.Min(1, rng.Next(0, absentLike + 1));
                presentLike -= lateCount;
                absentLike -= excusedCount;
                if (absentLike < 0) absentLike = 0;
            }

            var seq = new List<AttendanceStatus>(sessionCount);
            for (int i = 0; i < presentLike; i++) seq.Add(AttendanceStatus.Present);
            for (int i = 0; i < lateCount; i++) seq.Add(AttendanceStatus.Late);
            for (int i = 0; i < excusedCount; i++) seq.Add(AttendanceStatus.Excused);
            for (int i = 0; i < absentLike; i++) seq.Add(AttendanceStatus.Absent);
            // Sequence ölçüsünü tam tutmaq üçün düzəldiş
            while (seq.Count < sessionCount) seq.Add(AttendanceStatus.Present);
            if (seq.Count > sessionCount) seq = seq.Take(sessionCount).ToList();

            // Qarışdır (xronoloji düzgünlük üçün shuffle)
            for (int i = seq.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (seq[i], seq[j]) = (seq[j], seq[i]);
            }
            return seq;
        }
    }
}
