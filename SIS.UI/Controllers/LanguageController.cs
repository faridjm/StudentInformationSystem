using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SIS.UI.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult Set(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture)) return LocalRedirect(returnUrl ?? "/");

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = false,
                    HttpOnly = false,
                    Path = "/"
                });

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}
