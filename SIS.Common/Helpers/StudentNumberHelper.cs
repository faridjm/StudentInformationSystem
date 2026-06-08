using System.Text.RegularExpressions;

namespace SIS.Common.Helpers
{
    /// <summary>
    /// Student nömrəsi generatoru. Format: STU001, STU002, ..., STU999, STU1000.
    /// Silinmiş tələbələrin nömrələri təkrar istifadə olunmur.
    /// </summary>
    public static partial class StudentNumberHelper
    {
        /// <summary>
        /// Mövcud StudentNumber-lərin siyahısından növbəti unique nömrəni generate edir.
        /// existingNumbers: database-dəki BÜTÜN student-lərin (silinmiş daxil) nömrələri.
        /// </summary>
        public static string GenerateNext(IEnumerable<string?> existingNumbers)
        {
            int max = 0;
            foreach (var num in existingNumbers)
            {
                if (string.IsNullOrWhiteSpace(num)) continue;
                var match = StuPattern().Match(num);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var n))
                {
                    if (n > max) max = n;
                }
            }
            return $"STU{(max + 1):D3}";
        }

        /// <summary>Köhnə metod — compat üçün saxlanılır amma istifadə etmə.</summary>
        [Obsolete("Use GenerateNext(existingNumbers) instead")]
        public static string Generate(int enrollmentYear, int sequence)
        {
            return $"STU{sequence:D3}";
        }

        [GeneratedRegex(@"^STU(\d+)$", RegexOptions.IgnoreCase)]
        private static partial Regex StuPattern();
    }
}
