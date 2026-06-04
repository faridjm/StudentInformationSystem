using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIS.BLL.Services.Interfaces;
using SIS.Entities.Identity;

namespace SIS.BLL.Services.Implementations
{
    public class InstitutionalEmailService : IInstitutionalEmailService
    {
        private readonly UserManager<AppUser> _userManager;
        private const string Domain = "sis.az";

        // Azərbaycan hərflərinin normalize xəritəsi
        private static readonly Dictionary<char, char> AzMap = new()
        {
            { 'ə', 'e' }, { 'Ə', 'e' },
            { 'ö', 'o' }, { 'Ö', 'o' },
            { 'ü', 'u' }, { 'Ü', 'u' },
            { 'ş', 's' }, { 'Ş', 's' },
            { 'ç', 'c' }, { 'Ç', 'c' },
            { 'ğ', 'g' }, { 'Ğ', 'g' },
            { 'ı', 'i' }, { 'İ', 'i' },
        };

        public InstitutionalEmailService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public string NormalizeForEmail(string input)
        {
            var result = new System.Text.StringBuilder(input.Length);
            foreach (var ch in input)
            {
                if (AzMap.TryGetValue(ch, out var mapped))
                    result.Append(mapped);
                else
                    result.Append(char.ToLowerInvariant(ch));
            }
            return result.ToString();
        }

        public async Task<string> GenerateEmailAsync(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("Ad boş ola bilməz", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Soyad boş ola bilməz", nameof(lastName));

            // Normalize + yalnız hərfləri saxla
            var normFirst = new string(NormalizeForEmail(firstName.Trim()).Where(char.IsLetter).ToArray());
            var normLast  = new string(NormalizeForEmail(lastName.Trim()).Where(char.IsLetter).ToArray());

            if (normFirst.Length == 0 || normLast.Length == 0)
                throw new ArgumentException("Ad/soyad etibarlı hərflərdən ibarət olmalıdır");

            // "farid" + "m" → "faridm"
            var baseLocal = normFirst + normLast[0];

            // faridm1, faridm2, ... — unique olana qədər
            int counter = 1;
            string candidate;
            do
            {
                candidate = $"{baseLocal}{counter}@{Domain}";
                counter++;
            }
            while (await _userManager.Users.AnyAsync(u => u.Email == candidate));

            return candidate;
        }
    }
}
