using FluentValidation;
using FluentValidation.AspNetCore;
using SIS.API.Services;
using SIS.BLL.Mappings;
using SIS.BLL.Services.Implementations;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;

namespace SIS.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ICourseOfferingService, CourseOfferingService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IAcademicProgramService, AcademicProgramService>();
            services.AddScoped<IAcademicTermService, AcademicTermService>();
            services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
            services.AddHttpContextAccessor();
            services.AddScoped<IActivityLogger, ActivityLogger>();


            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
