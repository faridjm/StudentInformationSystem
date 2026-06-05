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

        public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            var apiBase = configuration["ApiSettings:BaseUrl"]!;
            services.AddTransient<AuthDelegatingHandler>();

            void Add<T>() where T : class =>
                services.AddRefitClient<T>(RefitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
                    .AddHttpMessageHandler<AuthDelegatingHandler>();

            Add<IAuthApi>();
            Add<IStudentApi>();
            Add<ITeacherApi>();
            Add<IDepartmentApi>();
            Add<ICourseApi>();
            Add<IGroupApi>();
            Add<IEnrollmentApi>();
            Add<IAssessmentApi>();
            Add<IGradeApi>();
            Add<IAttendanceApi>();
            Add<IAcademicYearApi>();
            Add<IClassSessionApi>();
            Add<IAnnouncementApi>();
            Add<INotificationApi>();
            Add<IActivityLogApi>();
            Add<IAssignmentApi>();
            Add<ISimulationApi>();
            Add<ITranscriptApi>();
            Add<IChatApi>();
            Add<IQuizApi>();
            Add<IAuditLogApi>();
            Add<IAdminAccessApi>();
            Add<ICourseMaterialApi>();
            Add<ITeacherDashboardApi>();
            Add<ICurriculumTemplateApi>();
            Add<IInstitutionalEmailApi>();
            Add<IArchiveApi>();
            Add<IPublicApi>();

            return services;
        }
    }
}
