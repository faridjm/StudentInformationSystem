using SIS.BLL.Helpers;
using SIS.BLL.Services.DTOs.Transcript;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// QR-əsaslı transcript doğrulama qeydlərini idarə edir.
    /// Calculation logic-ə toxunmur — yalnız hazır TranscriptDto dəyərlərini snapshot kimi saxlayır.
    /// </summary>
    public class TranscriptVerificationService : ITranscriptVerificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TranscriptVerificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TranscriptVerification> GetOrCreateAsync(TranscriptDto data, string lang)
        {
            lang = (lang ?? "az").Trim().ToLowerInvariant();
            if (lang != "en" && lang != "ru") lang = "az";

            var now = DateTime.UtcNow;

            // Eyni tələbə + qrup + tip üçün aktiv qeyd varsa onu istifadə et (stable code reuse).
            var existing = await _unitOfWork.TranscriptVerifications
                .GetActiveAsync(data.StudentId, data.GroupId, data.TranscriptType);

            if (existing != null)
            {
                // Snapshot dəyərlərini cari hesablama ilə yenilə (kod dəyişmir).
                existing.EnrollmentId = data.EnrollmentId;
                existing.Language = lang;
                existing.OfficialGrade = data.OfficialGrade;
                existing.GPA = data.GPA;
                existing.AttendancePercentage = data.AttendancePercentage;
                existing.CompletionLevel = data.CompletionLevel;
                existing.CertificateStatus = data.CertificateStatus;
                existing.IssuedAt = now;
                _unitOfWork.TranscriptVerifications.Update(existing);
                await _unitOfWork.SaveAsync();
                return existing;
            }

            // Yeni qeyd — unikal kod.
            string code = await GenerateUniqueCodeAsync();

            var record = new TranscriptVerification
            {
                VerificationCode = code,
                StudentId = data.StudentId,
                GroupId = data.GroupId,
                EnrollmentId = data.EnrollmentId,
                TranscriptType = data.TranscriptType,
                Language = lang,
                OfficialGrade = data.OfficialGrade,
                GPA = data.GPA,
                AttendancePercentage = data.AttendancePercentage,
                CompletionLevel = data.CompletionLevel,
                CertificateStatus = data.CertificateStatus,
                IssuedAt = now,
                ExpiresAt = null,
                IsRevoked = false,
                CreatedAt = now
            };

            await _unitOfWork.TranscriptVerifications.AddAsync(record);
            await _unitOfWork.SaveAsync();
            return record;
        }

        public async Task<TranscriptVerificationPublicDto> GetByCodeAsync(string code)
        {
            var dto = new TranscriptVerificationPublicDto { VerificationCode = code };

            if (string.IsNullOrWhiteSpace(code))
            {
                dto.Status = "Invalid";
                return dto;
            }

            var record = await _unitOfWork.TranscriptVerifications.GetByCodeAsync(code.Trim());
            if (record == null)
            {
                dto.Status = "Invalid";
                return dto;
            }

            if (record.IsRevoked)
            {
                dto.Status = "Revoked";
                return dto;
            }

            if (record.ExpiresAt.HasValue && record.ExpiresAt.Value < DateTime.UtcNow)
            {
                dto.Status = "Expired";
                return dto;
            }

            // Valid — public-safe sahələri doldur (enrollment-dən cari ad/kurs/qrup).
            dto.Status = "Valid";
            dto.TranscriptType = record.TranscriptType;
            dto.OfficialGrade = record.OfficialGrade;
            dto.GPA = record.GPA;
            dto.AttendancePercentage = record.AttendancePercentage;
            dto.CompletionLevel = record.CompletionLevel;
            dto.CertificateStatus = record.CertificateStatus;
            dto.IssuedAt = record.IssuedAt;
            dto.ExpiresAt = record.ExpiresAt;

            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(record.EnrollmentId);
            if (enrollment != null)
            {
                dto.StudentFullName = enrollment.Student?.AppUser?.FullName;
                dto.CourseName = enrollment.Group?.Course?.Name;
                dto.GroupName = enrollment.Group?.Name;
            }

            return dto;
        }

        public async Task<bool> RevokeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var record = await _unitOfWork.TranscriptVerifications.GetByCodeAsync(code.Trim());
            if (record == null || record.IsRevoked) return false;

            record.IsRevoked = true;
            record.RevokedAt = DateTime.UtcNow;
            _unitOfWork.TranscriptVerifications.Update(record);
            await _unitOfWork.SaveAsync();
            return true;
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                var code = VerificationCodeGenerator.Generate(12);
                var clash = await _unitOfWork.TranscriptVerifications.GetByCodeAsync(code);
                if (clash == null) return code;
            }
            // Çox nadir hal — uzunluğu artırıb daha bir cəhd
            return VerificationCodeGenerator.Generate(16);
        }
    }
}
