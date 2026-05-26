namespace SIS.BLL.Services.DTOs.AcademicYear
{
    /// <summary>
    /// Siyahı/detail üçün universal DTO. Termlərin sayı və cari status göstərilir.
    /// </summary>
    public class AcademicYearDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        /// <summary>Bu sezona bağlı semestrlərin sayı.</summary>
        public int TermCount { get; set; }

        /// <summary>Bu sezon hazırda davam edirmi (StartDate ≤ now ≤ EndDate).</summary>
        public bool IsCurrent { get; set; }
    }
}
