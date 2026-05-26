using SIS.Entities.Base;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Dərs sezonu (məs. "2026-27"). Bir sezonda bir neçə AcademicTerm (semestr) olur.
    /// </summary>
    public class AcademicYear : BaseEntity
    {
        /// <summary>Adı, məs. "2026-27" və ya "2026/2027".</summary>
        public string Name { get; set; } = null!;

        /// <summary>Sezonun ümumi başlanğıc tarixi (1-ci semestrin qeydiyyatından əvvəl).</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Sezonun ümumi bitiş tarixi (son semestrin qiymət təsdiqindən sonra).</summary>
        public DateTime EndDate { get; set; }

        /// <summary>Cari aktiv sezon yalnız bir dənə olmalıdır.</summary>
        public bool IsActive { get; set; } = false;

        public ICollection<AcademicTerm> Terms { get; set; } = new List<AcademicTerm>();
    }
}
