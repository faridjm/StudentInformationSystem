namespace SIS.API.Services
{
    /// <summary>
    /// Transcript / akademik PDF-lər üçün statik label tərcüməsi.
    /// 3 dil: az (default), en, ru. Yalnız statik label-lər və status text-ləri
    /// tərcümə olunur — database-dən gələn real data (ad, kurs, qrup və s.) toxunulmur.
    /// </summary>
    public static class PdfLocalization
    {
        public static string Normalize(string? lang)
        {
            lang = (lang ?? "az").Trim().ToLowerInvariant();
            return lang is "en" or "ru" ? lang : "az";
        }

        /// <summary>Verilmiş açar üçün dilə uyğun mətn qaytarır.</summary>
        public static string T(string key, string lang)
        {
            lang = Normalize(lang);
            if (_dict.TryGetValue(key, out var byLang) && byLang.TryGetValue(lang, out var val))
                return val;
            // Açar tapılmasa, açarın özünü qaytar (fail-safe)
            return key;
        }

        private static readonly Dictionary<string, Dictionary<string, string>> _dict = new()
        {
            ["Transcript"] = new() { ["az"] = "Akademik çıxarış", ["en"] = "Academic Transcript", ["ru"] = "Академическая выписка" },
            ["Platform"] = new() { ["az"] = "Rəqəmsal Akademiya İdarəetmə Platforması", ["en"] = "Digital Academy Management Platform", ["ru"] = "Платформа управления цифровой академией" },
            ["StudentInformation"] = new() { ["az"] = "Tələbə məlumatları", ["en"] = "Student Information", ["ru"] = "Информация о студенте" },
            ["CourseInformation"] = new() { ["az"] = "Kurs məlumatları", ["en"] = "Course Information", ["ru"] = "Информация о курсе" },
            ["FullName"] = new() { ["az"] = "Ad Soyad", ["en"] = "Full Name", ["ru"] = "ФИО" },
            ["StudentNumber"] = new() { ["az"] = "Tələbə №", ["en"] = "Student No.", ["ru"] = "Номер студента" },
            ["Email"] = new() { ["az"] = "Email", ["en"] = "Email", ["ru"] = "Email" },
            ["Course"] = new() { ["az"] = "Kurs / Proqram", ["en"] = "Course / Programme", ["ru"] = "Курс / Программа" },
            ["Group"] = new() { ["az"] = "Qrup", ["en"] = "Group", ["ru"] = "Группа" },
            ["Teacher"] = new() { ["az"] = "Müəllim", ["en"] = "Teacher", ["ru"] = "Преподаватель" },
            ["AcademicYear"] = new() { ["az"] = "Tədris ili", ["en"] = "Academic Year", ["ru"] = "Учебный год" },
            ["EnrollmentYear"] = new() { ["az"] = "Qeydiyyat ili", ["en"] = "Enrollment Year", ["ru"] = "Год зачисления" },
            ["Period"] = new() { ["az"] = "Dövr", ["en"] = "Period", ["ru"] = "Период" },
            ["GeneratedAt"] = new() { ["az"] = "Yaradılma tarixi", ["en"] = "Generated At", ["ru"] = "Дата формирования" },

            ["AttendanceSummary"] = new() { ["az"] = "Davamiyyət xülasəsi", ["en"] = "Attendance Summary", ["ru"] = "Сводка посещаемости" },
            ["Present"] = new() { ["az"] = "İştirak", ["en"] = "Present", ["ru"] = "Присутствовал" },
            ["Late"] = new() { ["az"] = "Gecikmə", ["en"] = "Late", ["ru"] = "Опоздал" },
            ["Absent"] = new() { ["az"] = "Qayıb", ["en"] = "Absent", ["ru"] = "Отсутствовал" },
            ["Excused"] = new() { ["az"] = "Üzrlü", ["en"] = "Excused", ["ru"] = "Уважительная причина" },
            ["TotalSessions"] = new() { ["az"] = "Cəmi dərs", ["en"] = "Total Sessions", ["ru"] = "Всего занятий" },
            ["TakenSessions"] = new() { ["az"] = "Keçirilmiş", ["en"] = "Taken Sessions", ["ru"] = "Учтенные занятия" },
            ["AttendancePercentage"] = new() { ["az"] = "Davamiyyət faizi", ["en"] = "Attendance Percentage", ["ru"] = "Процент посещаемости" },
            ["Attendance"] = new() { ["az"] = "Davamiyyət", ["en"] = "Attendance", ["ru"] = "Посещаемость" },
            ["AttendanceFormula"] = new()
            {
                ["az"] = "Davamiyyət faizi: (İştirak + Gecikmə × 0.5 + Üzrlü) / Keçirilmiş sessiyalar × 100",
                ["en"] = "Attendance calculation: (Present + Late × 0.5 + Excused) / Taken sessions × 100",
                ["ru"] = "Расчет посещаемости: (Присутствие + Опоздание × 0.5 + Уважительная) / Учтенные занятия × 100"
            },

            ["AssessmentSummary"] = new() { ["az"] = "Qiymətləndirmə xülasəsi", ["en"] = "Assessment Summary", ["ru"] = "Сводка оценивания" },
            ["Assessment"] = new() { ["az"] = "Qiymətləndirmə", ["en"] = "Assessment", ["ru"] = "Оценивание" },
            ["Type"] = new() { ["az"] = "Növ", ["en"] = "Type", ["ru"] = "Тип" },
            ["Date"] = new() { ["az"] = "Tarix", ["en"] = "Date", ["ru"] = "Дата" },
            ["Score"] = new() { ["az"] = "Bal", ["en"] = "Score", ["ru"] = "Балл" },
            ["MaxScore"] = new() { ["az"] = "Maksimum bal", ["en"] = "Max Score", ["ru"] = "Максимальный балл" },
            ["Weight"] = new() { ["az"] = "Çəki", ["en"] = "Weight", ["ru"] = "Вес" },
            ["Contribution"] = new() { ["az"] = "Nəticəyə təsir", ["en"] = "Contribution", ["ru"] = "Вклад" },
            ["Status"] = new() { ["az"] = "Status", ["en"] = "Status", ["ru"] = "Статус" },
            ["Graded"] = new() { ["az"] = "Qiymətləndirilib", ["en"] = "Graded", ["ru"] = "Оценено" },
            ["NotGraded"] = new() { ["az"] = "Qiymətləndirilməyib", ["en"] = "Not Graded", ["ru"] = "Не оценено" },

            ["HomeworkPerformance"] = new() { ["az"] = "Ev tapşırıqları", ["en"] = "Homework Performance", ["ru"] = "Домашние задания" },
            ["HomeworkNote"] = new() { ["az"] = "Bu bölmə GPA hesablamasına daxil deyil", ["en"] = "This section is not included in GPA", ["ru"] = "Этот раздел не входит в расчет GPA" },
            ["HomeworkAverage"] = new() { ["az"] = "Ev tapşırığı ortalaması", ["en"] = "Homework Average", ["ru"] = "Средний балл домашних заданий" },
            ["DueDate"] = new() { ["az"] = "Son tarix", ["en"] = "Due Date", ["ru"] = "Срок сдачи" },
            ["Link"] = new() { ["az"] = "Bağlantı", ["en"] = "Link", ["ru"] = "Связь" },
            ["Official"] = new() { ["az"] = "Rəsmi", ["en"] = "Official", ["ru"] = "Официальное" },
            ["Homework"] = new() { ["az"] = "Ev tapş.", ["en"] = "Homework", ["ru"] = "Домашнее" },

            ["FinalResult"] = new() { ["az"] = "Yekun akademik nəticə", ["en"] = "Final Academic Result", ["ru"] = "Итоговый академический результат" },
            ["OfficialGrade"] = new() { ["az"] = "Rəsmi qiymət", ["en"] = "Official Grade", ["ru"] = "Итоговая оценка" },
            ["GPA"] = new() { ["az"] = "GPA", ["en"] = "GPA", ["ru"] = "GPA" },
            ["Result"] = new() { ["az"] = "Nəticə", ["en"] = "Result", ["ru"] = "Результат" },
            ["PerformanceStatus"] = new() { ["az"] = "Performans statusu", ["en"] = "Performance Status", ["ru"] = "Статус успеваемости" },
            ["CertificateStatus"] = new() { ["az"] = "Sertifikat statusu", ["en"] = "Certificate Status", ["ru"] = "Статус сертификата" },
            ["CertificateCondition"] = new()
            {
                ["az"] = "Şərt: Qiymət ≥ 60% və Davamiyyət ≥ 70%",
                ["en"] = "Condition: Grade ≥ 60% and Attendance ≥ 70%",
                ["ru"] = "Условие: оценка ≥ 60% и посещаемость ≥ 70%"
            },

            ["Passed"] = new() { ["az"] = "Keçdi", ["en"] = "Passed", ["ru"] = "Сдал" },
            ["Failed"] = new() { ["az"] = "Keçmədi", ["en"] = "Failed", ["ru"] = "Не сдал" },
            ["Pending"] = new() { ["az"] = "Gözləmədə", ["en"] = "Pending", ["ru"] = "В ожидании" },
            ["Eligible"] = new() { ["az"] = "Uyğundur", ["en"] = "Eligible", ["ru"] = "Допущен" },
            ["NotEligible"] = new() { ["az"] = "Uyğun deyil", ["en"] = "Not Eligible", ["ru"] = "Не допущен" },

            ["FooterNote"] = new()
            {
                ["az"] = "Bu sənəd Acadex 360 sistemi tərəfindən avtomatik yaradılmışdır. Təsdiq üçün akademiya administrasiyası ilə əlaqə saxlayın.",
                ["en"] = "This document was generated automatically by Acadex 360. For verification, please contact the academy administration.",
                ["ru"] = "Этот документ был автоматически сформирован системой Acadex 360. Для подтверждения обратитесь в администрацию академии."
            },
            ["Page"] = new() { ["az"] = "Səhifə", ["en"] = "Page", ["ru"] = "Страница" },
            ["NoData"] = new() { ["az"] = "Məlumat yoxdur", ["en"] = "No data", ["ru"] = "Нет данных" }
        };
    }
}
