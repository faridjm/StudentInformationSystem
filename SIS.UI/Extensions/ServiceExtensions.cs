using Refit;
using SIS.UI.ApiClients;
using SIS.UI.Handlers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SIS.UI.Extensions
{
    public static class ServiceExtensions
    {
        private static readonly RefitSettings RefitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                })
        };

        public static IServiceCollection AddApiClients(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var apiBase = configuration["ApiSettings:BaseUrl"]!;

            services.AddTransient<AuthDelegatingHandler>();

            services.AddRefitClient<IAuthApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IStudentApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<ITeacherApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<ICourseApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<ICourseOfferingApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IEnrollmentApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IGradeApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAttendanceApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IDepartmentApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAcademicProgramApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAcademicYearApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAcademicTermApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IWeeklyScheduleApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IClassSessionApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAnnouncementApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<INotificationApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IActivityLogApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAssignmentApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<ISimulationApi>(RefitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            return services;
        }
    }
}
