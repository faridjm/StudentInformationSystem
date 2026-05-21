using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SIS.UI.ApiClients;
using System.Security.Claims;

namespace SIS.UI.Handlers
{
    public class AuthDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public AuthDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return await base.SendAsync(request, cancellationToken);

            var accessToken = context.User.FindFirst("AccessToken")?.Value;

            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshToken = context.User.FindFirst("RefreshToken")?.Value;

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    try
                    {
                        var authApi = _serviceProvider.GetRequiredService<IAuthApi>();
                        var newToken = await authApi.RefreshTokenAsync(refreshToken);

                        var claims = context.User.Claims.ToList();
                        claims.RemoveAll(c => c.Type == "AccessToken" || c.Type == "RefreshToken");
                        claims.Add(new Claim("AccessToken", newToken.AccessToken));
                        claims.Add(new Claim("RefreshToken", newToken.RefreshToken));

                        var identity = new ClaimsIdentity(claims,
                            CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await context.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal);

                        request.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue(
                                "Bearer", newToken.AccessToken);

                        response = await base.SendAsync(request, cancellationToken);
                    }
                    catch
                    {
                        await context.SignOutAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme);
                        context.Response.Redirect("/Auth/Login");
                    }
                }
            }

            return response;
        }
    }
}
