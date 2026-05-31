using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using SIS.API.Authorization.Handlers;
using SIS.API.Authorization.Requirements;
using SIS.BLL.Configuration;
using SIS.API.Extensions;
using SIS.API.Hubs;
using SIS.API.Middlewares;
using System.Text.Json.Serialization;
using QuestPDF.Infrastructure;

namespace SIS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            // Services
            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddAuth(builder.Configuration);
            builder.Services.AddServices();
            builder.Services.AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            builder.Services.AddOpenApi();
            builder.Services.AddSignalR();

            // JwtSettings strongly-typed
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("Jwt"));

            // CORS
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMvc", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("EnrollmentOwner",
                    p => p.Requirements.Add(new EnrollmentOwnerRequirement()));
                options.AddPolicy("GradeAuth",
                    p => p.Requirements.Add(new GradeAuthRequirement()));
                options.AddPolicy("StudentOwner",
                    p => p.Requirements.Add(new StudentOwnerRequirement()));
            });

            // Authorization Handlers
            builder.Services.AddScoped<IAuthorizationHandler, EnrollmentOwnerHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, GradeAuthHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, StudentOwnerHandler>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.Title = "SIS API";
                    options.Theme = ScalarTheme.DeepSpace;
                });
            }

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseCors("AllowMvc");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notification");

            app.SeedDataAsync().GetAwaiter().GetResult();

            app.Run();
        }
    }
}
