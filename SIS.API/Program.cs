using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using SIS.API.Authorization.Handlers;
using SIS.API.Authorization.Requirements;
using SIS.API.Extensions;
using SIS.API.Hubs;
using SIS.API.Middlewares;

namespace SIS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Services
            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddAuth(builder.Configuration);
            builder.Services.AddServices();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddSignalR();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
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
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notification");

            app.SeedDataAsync().GetAwaiter().GetResult();

            app.Run();
        }
    }
}
