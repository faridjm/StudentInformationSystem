using FluentValidation;
using FluentValidation.AspNetCore;
using SIS.API.Services;
using SIS.BLL.Mappings;
using SIS.BLL.Services.Implementations;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;

namespace SIS.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Global simulasiya rejimi: Admin tərəfindən idarə olunur, bütün istifadəçilərə tətbiq edilir
            services.AddSingleton<SimulationService>();
            services.AddScoped<IClock, SIS.API.Time.GlobalClock>();
            services.AddScoped<ICurrentUser, CurrentUser>();

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
            services.AddScoped<IAcademicYearService, AcademicYearService>();
            services.AddScoped<IAcademicTermService, AcademicTermService>();
            services.AddScoped<IWeeklySchedulePatternService, WeeklySchedulePatternService>();
            services.AddScoped<IClassSessionService, ClassSessionService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
            services.AddHttpContextAccessor();
            services.AddScoped<IActivityLogger, ActivityLogger>();
            services.AddScoped<IEmailService, EmailService>();


            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
