using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ITranscriptVerificationRepository : IGenericRepository<TranscriptVerification>
    {
        /// <summary>Doğrulama kodu ilə qeydi gətirir (revoke/expire yoxlaması service-də olur).</summary>
        Task<TranscriptVerification?> GetByCodeAsync(string code);

        /// <summary>Tələbə + qrup + tip üzrə aktiv (revoke olunmamış) qeydi gətirir.</summary>
        Task<TranscriptVerification?> GetActiveAsync(int studentId, int groupId, TranscriptType type);
    }
}
