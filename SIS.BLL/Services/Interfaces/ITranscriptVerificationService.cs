using SIS.BLL.Services.DTOs.Transcript;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Interfaces
{
    public interface ITranscriptVerificationService
    {
        /// <summary>
        /// Verilmiş transcript üçün aktiv doğrulama qeydini tapır (tələbə + qrup + tip üzrə)
        /// və ya yeni qeyd yaradır. Snapshot dəyərləri (grade/gpa/attendance/level/cert)
        /// yenilənir. Qaytarılan qeyddə VerificationCode olur.
        /// </summary>
        Task<TranscriptVerification> GetOrCreateAsync(TranscriptDto data, string lang);

        /// <summary>Public doğrulama: kod üzrə status (Valid/Invalid/Revoked/Expired) + təhlükəsiz məlumat.</summary>
        Task<TranscriptVerificationPublicDto> GetByCodeAsync(string code);

        /// <summary>Doğrulama qeydini ləğv edir (optional).</summary>
        Task<bool> RevokeAsync(string code);
    }
}
