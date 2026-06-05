using Microsoft.AspNetCore.Html;
using SIS.Entities.Enums;

namespace SIS.UI.Helpers
{
    /// <summary>
    /// Tədris formatı və online dərs statusu üçün rəngli badge-lər.
    /// Admin/Teacher/Student panellərində eyni görünüş üçün ortaq yer.
    /// </summary>
    public static class OnlineLessonBadges
    {
        public static IHtmlContent DeliveryBadge(DeliveryMode mode)
        {
            // Online=bənövşəyi, InPerson=mavi, Hybrid=firuzəyi
            var (bg, color, text, icon) = mode switch
            {
                DeliveryMode.Online => ("rgba(147,51,234,0.15)", "#9333ea", "Online", "fa-video"),
                DeliveryMode.Hybrid => ("rgba(20,184,166,0.15)", "#14b8a6", "Hibrid", "fa-laptop-house"),
                _                   => ("rgba(59,130,246,0.15)", "#3b82f6", "Əyani", "fa-chalkboard")
            };
            return Badge(bg, color, text, icon);
        }

        public static IHtmlContent StatusBadge(OnlineLessonStatus status)
        {
            // Scheduled=mavi, Live=yaşıl, Ended=boz, Cancelled=qırmızı
            var (bg, color, text, icon) = status switch
            {
                OnlineLessonStatus.Live      => ("rgba(34,197,94,0.15)", "#22c55e", "Canlı", "fa-circle"),
                OnlineLessonStatus.Ended     => ("rgba(107,114,128,0.15)", "#6b7280", "Bitdi", "fa-stop-circle"),
                OnlineLessonStatus.Cancelled => ("rgba(239,68,68,0.15)", "#ef4444", "Ləğv", "fa-ban"),
                _                            => ("rgba(59,130,246,0.15)", "#3b82f6", "Planlı", "fa-clock")
            };
            return Badge(bg, color, text, icon);
        }

        public static string PlatformName(OnlinePlatform? platform) => platform switch
        {
            OnlinePlatform.GoogleMeet     => "Google Meet",
            OnlinePlatform.Zoom           => "Zoom",
            OnlinePlatform.MicrosoftTeams => "Microsoft Teams",
            OnlinePlatform.Custom         => "Digər",
            _                             => "—"
        };

        /// <summary>Meeting URL-in host hissəsini göstərir (tam linki gizlədir).</summary>
        public static string MaskUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "—";
            return Uri.TryCreate(url, UriKind.Absolute, out var u) ? u.Host : "link";
        }

        private static IHtmlContent Badge(string bg, string color, string text, string icon)
        {
            var html = $"<span style=\"display:inline-flex; align-items:center; gap:4px; background:{bg}; color:{color}; "
                     + $"padding:2px 8px; border-radius:6px; font-size:11px; font-weight:600;\">"
                     + $"<i class=\"fas {icon}\" style=\"font-size:9px;\"></i>{text}</span>";
            return new HtmlString(html);
        }
    }
}
