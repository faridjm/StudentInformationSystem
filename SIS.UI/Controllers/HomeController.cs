using Microsoft.AspNetCore.Mvc;
using SIS.UI.Models;
using System.Diagnostics;

namespace SIS.UI.Controllers
{

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("Moderator"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                if (User.IsInRole("Teacher"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Teacher" });

                if (User.IsInRole("Student"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Student" });
            }

            return RedirectToAction("Login", "Auth");
        }
    }
}