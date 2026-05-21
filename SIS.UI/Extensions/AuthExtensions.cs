using Microsoft.AspNetCore.Authentication.Cookies;

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("SuperAdmin", "Admin", "Moderator"));

                options.AddPolicy("TeacherOnly", policy =>
                    policy.RequireRole("Teacher"));

                options.AddPolicy("StudentOnly", policy =>
                    policy.RequireRole("Student"));
            });

            return services;
        }
    }
}
