using System.Security.Cryptography;

namespace SIS.BLL.Helpers
{
    /// <summary>
    /// Kriptoqrafik təhlükəsiz, təxmin edilməsi çətin doğrulama kodu yaradır.
    /// Qarışıq simvollar (0/O, 1/I) çıxarılıb ki, oxunaqlı olsun.
    /// </summary>
    public static class VerificationCodeGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 32 simvol, ambiguous yox

        public static string Generate(int length = 12)
        {
            if (length < 8) length = 8;
            var chars = new char[length];
            var bytes = RandomNumberGenerator.GetBytes(length);
            for (int i = 0; i < length; i++)
                chars[i] = Alphabet[bytes[i] % Alphabet.Length];
            return new string(chars);
        }
    }
}
