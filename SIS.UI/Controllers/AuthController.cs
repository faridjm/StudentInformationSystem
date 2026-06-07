using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Refit;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.DTOs.AuditLog;
using SIS.UI.ApiClients;
using System.Security.Claims;

namespace SIS.UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthApi _authApi;
        private readonly IStringLocalizer<SharedResource> _l;
        private readonly IAuditLogApi _auditApi;

        public AuthController(IAuthApi authApi, IStringLocalizer<SharedResource> localizer, IAuditLogApi auditApi)
        {
            _authApi = authApi;
            _l = localizer;
            _auditApi = auditApi;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var token = await _authApi.LoginAsync(dto);

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim("AccessToken", token.AccessToken),
                new Claim("RefreshToken", token.RefreshToken),
            };

                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token.AccessToken);

                foreach (var claim in jwtToken.Claims)
                {
                    if (claim.Type == "role" ||
                        claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));

                    if (claim.Type == "unique_name" ||
                        claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                        claims.Add(new Claim(ClaimTypes.Name, claim.Value));

                    if (claim.Type == "nameid" ||
                        claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                }

                var identity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = dto.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch (ApiException ex)
            {
                var message = _l["InvalidCredentials"].Value;
                try
                {
                    var errorContent = ex.Content;
                    if (!string.IsNullOrEmpty(errorContent))
                    {
                        var error = System.Text.Json.JsonDocument.Parse(errorContent);
                        if (error.RootElement.TryGetProperty("message", out var msg))
                            message = msg.GetString() ?? message;
                    }
                }
                catch { }

                ModelState.AddModelError("", message);
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try { await _authApi.LogoutAsync(); } catch { }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> AccessDenied(string? returnUrl = null)
        {
            // İstifadəçi hansı panelə girməyə cəhd etdiyini path-dan müəyyən edirik.
            // Audit log sistemə təhlükəsizlik hadisəsi kimi yazılır (best-effort).
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var path = (returnUrl ?? Request.Query["ReturnUrl"].ToString() ?? "").ToLowerInvariant();
                    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                               ?? (User.IsInRole("SuperAdmin") ? "SuperAdmin"
                                   : User.IsInRole("Admin") ? "Admin"
                                   : User.IsInRole("Moderator") ? "Moderator"
                                   : User.IsInRole("Teacher") ? "Teacher"
                                   : User.IsInRole("Student") ? "Student" : "Unknown");

                    string action;
                    string description;
                    if (path.StartsWith("/publicadmin"))
                    {
                        action = "AccessDeniedToPublicAdmin";
                        description = $"{role} istifadəçisi PublicAdmin panelinə girməyə cəhd etdi: {returnUrl}";
                    }
                    else if (path.StartsWith("/admin"))
                    {
                        // Admin rolu var amma access aktiv deyil — fərqli action
                        if (User.IsInRole("Admin"))
                        {
                            action = "AdminAccessDeniedInactive";
                            description = $"Admin {User.Identity?.Name} aktiv access olmadan Academic Admin panelinə girməyə cəhd etdi: {returnUrl}";
                        }
                        else
                        {
                            action = "AccessDeniedToAcademicAdmin";
                            description = $"{role} istifadəçisi Academic Admin panelinə girməyə cəhd etdi: {returnUrl}";
                        }
                    }
                    else
                    {
                        // Naməlum sahə — sadəcə skip et.
                        ViewBag.ReturnUrl = returnUrl;
                        return View();
                    }

                    await _auditApi.LogSecurityEventAsync(new SecurityEventDto
                    {
                        Action = action,
                        Description = description
                    });
                }
            }
            catch
            {
                // Audit yazılması istifadəçi cavabını pozmamalıdır.
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", _l["EnterEmail"].Value);
                return View();
            }
            try { await _authApi.ForgotPasswordAsync(new SIS.BLL.Services.DTOs.Auth.ForgotPasswordDto { Email = email }); } catch { }
            ViewBag.Sent = true;
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordDto { Email = email, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _authApi.ResetPasswordAsync(dto);
                ViewBag.Done = true;
                return View(dto);
            }
            catch (ApiException ex)
            {
                var message = _l["ErrorOccurred"].Value;
                try
                {
                    var err = System.Text.Json.JsonDocument.Parse(ex.Content ?? "");
                    if (err.RootElement.TryGetProperty("message", out var msg))
                        message = msg.GetString() ?? message;
                }
                catch { }
                ModelState.AddModelError("", message);
                return View(dto);
            }
        }
    }
}