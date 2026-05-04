using SIS.BLL.Services.Implementations;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.BLL.Mappings;

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


            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            return services;
        }
    }
}
