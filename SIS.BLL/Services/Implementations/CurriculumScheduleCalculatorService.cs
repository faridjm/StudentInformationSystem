using SIS.BLL.Services.DTOs.Groups;
using SIS.BLL.Services.Interfaces;

namespace SIS.BLL.Services.Implementations
{
    public class CurriculumScheduleCalculatorService : ICurriculumScheduleCalculatorService
    {
        private readonly ICurriculumTemplateService _templates;

        // Sonsuz dövrdən qorunma: ~40 il (məsələn yalnız 1 günlük, çox sessiyalı hal).
        private const int MaxIterations = 366 * 40;

        public CurriculumScheduleCalculatorService(ICurriculumTemplateService templates)
        {
            _templates = templates;
        }

        public DateTime CalculateEndDate(DateTime startDate, IEnumerable<DayOfWeek> scheduleDays, int totalSessions)
        {
            var days = (scheduleDays ?? Enumerable.Empty<DayOfWeek>()).ToList();

            if (days.Count == 0)
                throw new ArgumentException("Ən azı bir dərs günü seçilməlidir.", nameof(scheduleDays));
            if (totalSessions <= 0)
                throw new ArgumentException("Dərs sayı 0-dan böyük olmalıdır.", nameof(totalSessions));

            // Hər həftə günü üçün dərs sayı (eyni gündə birdən çox vaxt slotu ola bilər) —
            // GenerateSessionsAsync ilə eyni accrual məntiqi.
            var perWeekday = new int[7];
            foreach (var d in days)
                perWeekday[(int)d]++;

            // StartDate-in özü seçilmiş gündürsə, 1-ci dərs kimi sayılır.
            var current = DateOnly.FromDateTime(startDate.Date);
            int counted = 0;

            for (int guard = 0; guard <= MaxIterations; guard++)
            {
                counted += perWeekday[(int)current.DayOfWeek];
                if (counted >= totalSessions)
                    return current.ToDateTime(TimeOnly.MinValue);
                current = current.AddDays(1);
            }

            throw new InvalidOperationException("Bitmə tarixi hesablanmadı (çox böyük dərs sayı).");
        }

        public async Task<CurriculumScheduleResult> CalculateAsync(CurriculumScheduleRequest request)
        {
            var days = request.ScheduleDays ?? new List<DayOfWeek>();
            var distinctDays = days.Distinct().ToList();

            if (distinctDays.Count == 0)
                return new CurriculumScheduleResult { Success = false, Message = "Ən azı bir dərs günü seçin." };

            // Dərs sayını həll et: əvvəl birbaşa TotalSessions, sonra şablon.
            int totalSessions = request.TotalSessions ?? 0;

            if (totalSessions <= 0 && request.CurriculumTemplateId.HasValue)
            {
                var detail = await _templates.GetDetailAsync(request.CurriculumTemplateId.Value);
                totalSessions = detail?.Items?.Count ?? 0;
            }

            if (totalSessions <= 0)
                return new CurriculumScheduleResult
                {
                    Success = false,
                    Message = "Dərs sayı təyin edilmədi. Tədris şablonu seçin və ya dərs sayı göstərin."
                };

            var endDate = CalculateEndDate(request.StartDate, days, totalSessions);

            return new CurriculumScheduleResult
            {
                Success = true,
                StartDate = request.StartDate.Date,
                EndDate = endDate,
                TotalSessions = totalSessions,
                DaysPerWeek = distinctDays.Count
            };
        }
    }
}
