using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Refit;
using SIS.BLL.Services.DTOs.Auth;
using SIS.UI.ApiClients;
using System.Security.Claims;

namespace SIS.UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthApi _authApi;

        public AuthController(IAuthApi authApi)
        {
            _authApi = authApi;
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
                var message = "Email və ya şifrə yanlışdır";
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

        public IActionResult AccessDenied() => View();
    }
}