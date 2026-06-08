using Microsoft.AspNetCore.Identity;
using SIS.Common.Helpers;
using SIS.Entities.Identity;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Helpers
{
    /// <summary>
    /// Bütün istifadəçi tiplərində (Student, Teacher, Admin, Moderator, SuperAdmin)
    /// telefon nömrəsinin AppUser səviyyəsində unique olmasını təmin edir.
    /// Normalize + yoxlama bir yerdə.
    /// </summary>
    public static class PhoneUniqueValidator
    {
        /// <summary>
        /// Telefon nömrəsini normalize edir və eyni nömrə ilə başqa istifadəçi varsa xəta atır.
        /// </summary>
        /// <param name="userManager">Identity UserManager</param>
        /// <param name="rawPhone">İstifadəçinin daxil etdiyi nömrə (istənilən formatda)</param>
        /// <param name="currentUserId">Update zamanı öz Id-si (nullable — create zamanı null)</param>
        /// <returns>Normalized phone (və ya null əgər boşdursa)</returns>
        public static async Task<string?> ValidateAndNormalizeAsync(
            UserManager<AppUser> userManager, string? rawPhone, string? currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(rawPhone))
                return null;

            var normalized = PhoneNormalizer.Normalize(rawPhone);
            if (normalized == null)
                return null; // Tanınmayan format — amma boş deyilsə olduğu kimi saxla

            // Eyni normalized phone ilə başqa aktiv user varmı?
            // UserManager.Users IQueryable-dir — DB query atır
            var existing = userManager.Users
                .Where(u => u.PhoneNumber != null && u.IsActive)
                .AsEnumerable()
                .FirstOrDefault(u => PhoneNormalizer.Normalize(u.PhoneNumber) == normalized);

            if (existing != null && existing.Id != currentUserId)
                throw new BadRequestException("Bu telefon nömrəsi artıq istifadə olunur.");

            return normalized;
        }
    }
}
