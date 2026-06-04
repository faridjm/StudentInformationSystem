using SIS.BLL.Services.DTOs.Groups;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Qrupun bitmə tarixini (EndDate) seçilmiş dərs günləri və dərs sayına görə hesablayır.
    /// StartDate-dən başlayaraq yalnız seçilmiş həftə günlərini sayır və N-ci dərsin real tarixini tapır.
    /// Həm preview endpoint-də, həm də qrup yaradılarkən server tərəfdə istifadə olunur.
    /// </summary>
    public interface ICurriculumScheduleCalculatorService
    {
        /// <summary>
        /// Saf hesablama: StartDate-dən başlayaraq yalnız <paramref name="scheduleDays"/> günlərini sayaraq
        /// <paramref name="totalSessions"/>-ci dərsin tarixini qaytarır.
        /// </summary>
        DateTime CalculateEndDate(DateTime startDate, IEnumerable<DayOfWeek> scheduleDays, int totalSessions);

        /// <summary>
        /// Dərs sayını curriculum şablonundan (və ya birbaşa TotalSessions-dan) həll edib EndDate hesablayır.
        /// Səhv girişlərdə Success=false və izahlı Message qaytarır (exception atmır) — AJAX preview üçün uyğundur.
        /// </summary>
        Task<CurriculumScheduleResult> CalculateAsync(CurriculumScheduleRequest request);
    }
}
