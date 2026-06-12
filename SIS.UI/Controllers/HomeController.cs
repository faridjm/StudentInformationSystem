using Microsoft.AspNetCore.Mvc;

namespace SIS.UI.Controllers
{
    /// <summary>
    /// Login uğurla başa çatdıqdan sonra istifadəçi roluna görə uyğun panelə yönləndirilir.
    /// Prioritet: SuperAdmin → Moderator → Admin → Teacher → Student → Public.
    /// Admin yalnız aktiv access grant olduqda Academic Admin panelə daxil ola bilər;
    /// əks halda AccessDenied göstərilir (handler-də fail-closed).
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Public");

            // SuperAdmin → Academic Admin paneli
            if (User.IsInRole("SuperAdmin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            // Moderator → birbaşa PublicAdmin paneli
            if (User.IsInRole("Moderator"))
                return RedirectToAction("Index", "Dashboard", new { area = "PublicAdmin" });

            // Admin → Academic Admin paneli (handler aktiv access yoxlayır;
            // access yoxdursa avtomatik AccessDenied səhifəsi göstərilir).
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (User.IsInRole("Teacher"))
                return RedirectToAction("Index", "Dashboard", new { area = "Teacher" });

            if (User.IsInRole("Student"))
                return RedirectToAction("Index", "Dashboard", new { area = "Student" });

            if (User.IsInRole("Parent"))
                return RedirectToAction("Index", "Dashboard", new { area = "Parent" });

            // Tanınmayan rol — public sayta düş.
            return RedirectToAction("Index", "Public");
        }
    }
}
