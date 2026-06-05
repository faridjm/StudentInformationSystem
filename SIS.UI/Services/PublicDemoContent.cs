using SIS.UI.ViewModels;

namespace SIS.UI.Services
{
    /// <summary>
    /// Public marketing saytının DEMO/MOCK məzmunu (database-də saxlanmır).
    /// Yalnız tanıtım bölmələri üçün: akademiya tarixi, otaqlar/şərait, uğurlu məzunlar.
    /// Real akademik data (kurs, qrup, müəllim) bura DAXİL DEYİL — o, API/servisdən gəlir.
    /// </summary>
    public static class PublicDemoContent
    {
        public static List<PublicFacilityViewModel> Facilities() => new()
        {
            new() { Name = "Lecture Room A", Type = "Classroom", Capacity = 20, Icon = "fa-chalkboard-user", Grad = 0,
                Description = "İnteraktiv dərslər və mentor rəhbərliyi ilə sessiyalar." },
            new() { Name = "Computer Lab", Type = "Lab", Capacity = 18, Icon = "fa-laptop-code", Grad = 1,
                Description = "Praktiki proqramlaşdırma və layihə işləri." },
            new() { Name = "Project Room", Type = "Collaboration", Capacity = 12, Icon = "fa-people-group", Grad = 2,
                Description = "Komanda əsaslı layihə inkişafı və təqdimatlar." },
            new() { Name = "Mentoring Room", Type = "Meeting", Capacity = 6, Icon = "fa-comments", Grad = 3,
                Description = "Fərdi geri bildiriş və karyera yönləndirmə sessiyaları." },
            new() { Name = "Online Learning Corner", Type = "Hybrid", Capacity = 30, Icon = "fa-wifi", Grad = 4,
                Description = "Onlayn dəstək və hibrid öyrənmə imkanları." }
        };

        public static List<PublicGraduateViewModel> Graduates() => new()
        {
            new() { FullName = "Aysel Kərimova", Course = "Full Stack Development", Year = 2024, Grad = 0,
                Position = "Junior .NET Developer", Story = "Acadex-də real layihələr üzərində işləyərək ilk iş təklifini məzun olmazdan əvvəl aldı." },
            new() { FullName = "Murad Həsənli", Course = "Data Analytics", Year = 2023, Grad = 1,
                Position = "Data Analyst", Story = "Strukturlaşdırılmış kurikulum və layihə portfeli onun analitika karyerasının təməli oldu." },
            new() { FullName = "Nigar Əliyeva", Course = "UI/UX Design", Year = 2024, Grad = 2,
                Position = "Product Designer", Story = "Mentor dəstəyi və real dizayn tapşırıqları ilə güclü portfolio formalaşdırdı." },
            new() { FullName = "Fərid Məmmədov", Course = "Backend Development", Year = 2025, Grad = 3,
                Position = "ASP.NET Core Developer", Story = "Layihə əsaslı qiymətləndirmə sistemi onu real komanda işinə hazırladı." }
        };

        public static List<PublicTimelineItem> Timeline() => new()
        {
            new() { Year = "2021", Text = "Acadex-in yaradılması" },
            new() { Year = "2022", Text = "İlk proqramlaşdırma qrupları" },
            new() { Year = "2023", Text = "Data Analytics və UI/UX istiqamətləri əlavə olundu" },
            new() { Year = "2024", Text = "Layihə əsaslı qiymətləndirmə sistemi tətbiq edildi" },
            new() { Year = "2025", Text = "Acadex SIS daxili idarəetmə platforması quruldu" },
            new() { Year = "2026", Text = "Role-based digital academy modelinin təqdimatı" }
        };

        public static List<string> Methodology() => new()
        {
            "Strukturlaşdırılmış kurikulum şablonları",
            "Davamiyyət izləməsi",
            "Quiz və tapşırıq sistemi",
            "GPA və transkript",
            "Layihə əsaslı qiymətləndirmə"
        };

        public const string Story =
            "Acadex 2021-ci ildə praktiki və layihə əsaslı tədris modeli yaratmaq məqsədilə fəaliyyətə başladı. " +
            "İlk illərdə proqramlaşdırma və data analitika üzrə kiçik qruplarla başlayan akademiya, qısa müddətdə " +
            "strukturlaşdırılmış kurikulum, real layihə tapşırıqları və mentor dəstəyi ilə seçilən tədris mərkəzinə çevrildi.";

        public const string Mission =
            "Praktiki bacarıqları real layihələr üzərində formalaşdırmaq və tələbələri əmək bazarına hazırlamaq.";

        public const string Vision =
            "Texnologiya təhsilində ölçülə bilən nəticə, şəffaf qiymətləndirmə və real portfolio əsaslı inkişaf modeli qurmaq.";
    }

    /// <summary>Şəkil olmadıqda peşəkar görünüş üçün ikon + qradiyent seçimi (broken image olmasın).</summary>
    public static class PublicVisuals
    {
        public static string CourseIcon(string? name, string? dept)
        {
            var s = ((name ?? "") + " " + (dept ?? "")).ToLowerInvariant();
            if (s.Contains("data") || s.Contains("analy")) return "fa-chart-line";
            if (s.Contains("design") || s.Contains("ui") || s.Contains("ux")) return "fa-pen-nib";
            if (s.Contains("web") || s.Contains("front")) return "fa-code";
            if (s.Contains("back") || s.Contains(".net") || s.Contains("api") || s.Contains("server")) return "fa-server";
            if (s.Contains("mobile") || s.Contains("android") || s.Contains("ios")) return "fa-mobile-screen-button";
            if (s.Contains("market")) return "fa-bullhorn";
            if (s.Contains("game")) return "fa-gamepad";
            if (s.Contains("network") || s.Contains("cyber") || s.Contains("security")) return "fa-shield-halved";
            return "fa-graduation-cap";
        }

        public static int Grad(int seed) => Math.Abs(seed) % 6;
    }
}
