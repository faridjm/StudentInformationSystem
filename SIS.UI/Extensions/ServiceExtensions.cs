using Refit;
using SIS.UI.ApiClients;
using SIS.UI.Handlers;

namespace SIS.UI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApiClients(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var apiBase = configuration["ApiSettings:BaseUrl"]!;

            services.AddTransient<AuthDelegatingHandler>();

            services.AddRefitClient<IAuthApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase));

            services.AddRefitClient<IStudentApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<ITeacherApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<ICourseApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<ICourseOfferingApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IEnrollmentApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IGradeApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAttendanceApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IDepartmentApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAcademicProgramApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAcademicYearApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAcademicTermApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IAnnouncementApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<INotificationApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            services.AddRefitClient<IActivityLogApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                .AddHttpMessageHandler<AuthDelegatingHandler>();

            return services;
        }
    }
}
