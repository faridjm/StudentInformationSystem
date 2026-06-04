namespace SIS.BLL.Services.DTOs.Groups
{
    /// <summary>
    /// Calculated EndDate üçün giriş: başlama tarixi, seçilmiş həftəlik dərs günləri,
    /// və dərs sayı (birbaşa və ya curriculum şablonundan götürülür).
    /// </summary>
    public class CurriculumScheduleRequest
    {
        public DateTime StartDate { get; set; }

        /// <summary>Seçilmiş həftəlik dərs günləri (DayOfWeek dəyərləri).</summary>
        public List<DayOfWeek> ScheduleDays { get; set; } = new();

        /// <summary>Dərs sayı bu şablondan götürülür (Items sayı). İstəyə bağlı.</summary>
        public int? CurriculumTemplateId { get; set; }

        /// <summary>Şablon olmadıqda və ya əvəz etmək üçün birbaşa dərs sayı. İstəyə bağlı.</summary>
        public int? TotalSessions { get; set; }
    }

    /// <summary>Calculated EndDate nəticəsi.</summary>
    public class CurriculumScheduleResult
    {
        public bool Success { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalSessions { get; set; }
        public int DaysPerWeek { get; set; }
        public string? Message { get; set; }
    }
}
