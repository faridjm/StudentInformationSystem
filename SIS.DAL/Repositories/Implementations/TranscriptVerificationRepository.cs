using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class TranscriptVerificationRepository
        : GenericRepository<TranscriptVerification>, ITranscriptVerificationRepository
    {
        public TranscriptVerificationRepository(AppDbContext context) : base(context) { }

        public Task<TranscriptVerification?> GetByCodeAsync(string code)
            => _dbSet.FirstOrDefaultAsync(v => v.VerificationCode == code);

        public Task<TranscriptVerification?> GetActiveAsync(int studentId, int groupId, TranscriptType type)
            => _dbSet
                .Where(v => v.StudentId == studentId
                            && v.GroupId == groupId
                            && v.TranscriptType == type
                            && !v.IsRevoked)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
    }
}
