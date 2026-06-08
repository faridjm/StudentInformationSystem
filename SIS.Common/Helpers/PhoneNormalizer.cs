using System.Text.RegularExpressions;

namespace SIS.Common.Helpers
{
    /// <summary>
    /// Azərbaycan telefon nömrələri üçün normalizasiya helper-i.
    /// Bütün formatlar vahid +994XXXXXXXXX formatına gətirilir.
    /// Unique yoxlama bu normalized dəyər üzərindən aparılır.
    /// </summary>
    public static partial class PhoneNormalizer
    {
        /// <summary>
        /// Nömrəni normalize edir → +994XXXXXXXXX (yalnız rəqəmlər + prefix).
        /// null/boş input → null qaytarır (optional phone).
        /// </summary>
        public static string? Normalize(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            // Yalnız rəqəmləri saxla (boşluq, tire, mötərizə, +, / silinir)
            var digits = DigitsOnly().Replace(phone, "");

            if (digits.Length == 0) return null;

            // 994XXXXXXXXX (12 rəqəm) → +994XXXXXXXXX
            if (digits.Length == 12 && digits.StartsWith("994"))
                return "+" + digits;

            // 0XXXXXXXXX (10 rəqəm, 0 ilə başlayır) → +994XXXXXXXXX
            if (digits.Length == 10 && digits.StartsWith("0"))
                return "+994" + digits[1..];

            // XXXXXXXXX (9 rəqəm — ölkə kodu və 0 yoxdur) → +994XXXXXXXXX
            if (digits.Length == 9 && (digits.StartsWith("50") || digits.StartsWith("51") ||
                digits.StartsWith("55") || digits.StartsWith("70") || digits.StartsWith("77") ||
                digits.StartsWith("10") || digits.StartsWith("12")))
                return "+994" + digits;

            // Artıq + ilə başlayıb 994XXXXXXXXX (13 rəqəm aradan əlavə olunmuşdursa)
            if (phone.TrimStart().StartsWith("+") && digits.Length == 12 && digits.StartsWith("994"))
                return "+" + digits;

            // Tanınmayan format — olduğu kimi saxla (amma rəqəmlərə sadələşdir)
            if (digits.Length >= 7)
                return digits.Length >= 12 ? "+" + digits : digits;

            return null;
        }

        /// <summary>
        /// Normalized nömrə +994XXXXXXXXX formatına uyğundurmu?
        /// </summary>
        public static bool IsValidAzerbaijanPhone(string? normalized)
        {
            if (string.IsNullOrWhiteSpace(normalized)) return false;
            return AzPhonePattern().IsMatch(normalized);
        }

        [GeneratedRegex(@"[^\d]")]
        private static partial Regex DigitsOnly();

        [GeneratedRegex(@"^\+994\d{9}$")]
        private static partial Regex AzPhonePattern();
    }
}
