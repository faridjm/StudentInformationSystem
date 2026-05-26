using SIS.Common.Time;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Helpers
{
    /// <summary>
    /// Servislərdə TermPhase əsaslı icazə yoxlamaları üçün ortaq köməkçi.
    /// Köhnə Term-lər (AcademicYearId = null) bu yoxlamadan azaddır — geriyə uyğunluq.
    /// </summary>
    public static class TermPhaseGuard
    {
        public static void EnsurePhase(
            IClock clock,
            AcademicTerm term,
            string action,
            params TermPhase[] allowedPhases)
        {
            // Köhnə (legacy) Term-lərə qayda tətbiq olunmur
            if (!term.AcademicYearId.HasValue) return;

            var phase = term.GetCurrentPhase(clock.UtcNow);
            if (!allowedPhases.Contains(phase))
            {
                var allowedNames = string.Join(", ", allowedPhases.Select(PhaseName));
                throw new BadRequestException(
                    $"{action} yalnız {allowedNames} fazasında mümkündür. " +
                    $"Cari faza: {PhaseName(phase)}");
            }
        }

        public static string PhaseName(TermPhase phase) => phase switch
        {
            TermPhase.Planning => "Planlama",
            TermPhase.Registration => "Qeydiyyat",
            TermPhase.Preparation => "Hazırlıq",
            TermPhase.Active => "Dərs gedir",
            TermPhase.ExamPeriod => "İmtahan",
            TermPhase.GradeReview => "Qiymət təsdiqi",
            TermPhase.Closed => "Bağlı",
            _ => phase.ToString()
        };
    }
}
