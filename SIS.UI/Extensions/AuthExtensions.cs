using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SIS.UI.Authorization;

namespace SIS.UI.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            // Admin panel girişini idarə edən handler (SuperAdmin həmişə, Admin yalnız aktiv grant ilə)
            services.AddScoped<IAuthorizationHandler, AdminPanelAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AdminPanelRequirement());
                });

                options.AddPolicy("TeacherOnly", policy =>
                    policy.RequireRole("Teacher"));

                options.AddPolicy("StudentOnly", policy =>
                    policy.RequireRole("Student"));

                // PublicAdmin panel — SuperAdmin və Moderator daxil ola bilər.
                // Admin / Teacher / Student / anonymous DENY.
                options.AddPolicy("PublicAdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("SuperAdmin", "Moderator");
                });
            });

            return services;
        }
    }
}
