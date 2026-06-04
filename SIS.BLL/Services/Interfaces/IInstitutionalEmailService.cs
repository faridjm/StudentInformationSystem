namespace SIS.BLL.Services.Interfaces
{
    public interface IInstitutionalEmailService
    {
        /// <summary>
        /// Ad + soyad əsasında unique @sis.az email generate edir.
        /// "Farid Mammadov" → "faridm1@sis.az" (faridm1 varsa → faridm2, ...)
        /// </summary>
        Task<string> GenerateEmailAsync(string firstName, string lastName);

        /// <summary>
        /// Azərbaycan hərflərini normalize edir: ə→e, ö→o, ü→u, ş→s, ç→c, ğ→g, ı→i
        /// </summary>
        string NormalizeForEmail(string input);
    }
}
