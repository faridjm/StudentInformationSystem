using FluentValidation;
using FluentValidation.AspNetCore;
using SIS.API.Services;
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
            services.AddSingleton<SimulationService>();
            services.AddScoped<IClock, SIS.API.Time.GlobalClock>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IAssessmentService, AssessmentService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAcademicYearService, AcademicYearService>();
            services.AddScoped<IClassSessionService, ClassSessionService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<ITranscriptService, TranscriptService>();
            services.AddScoped<ITranscriptPdfService, TranscriptPdfService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IAdminAccessService, AdminAccessService>();
            services.AddScoped<ICourseMaterialService, CourseMaterialService>();
            services.AddScoped<ITeacherDashboardService, TeacherDashboardService>();
            services.AddScoped<ICurriculumTemplateService, CurriculumTemplateService>();
            services.AddScoped<ICurriculumScheduleCalculatorService, CurriculumScheduleCalculatorService>();
            services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
            services.AddScoped<IChatRealtimeNotifier, ChatRealtimeNotifier>();
            services.AddScoped<IOnlineClassNotifier, OnlineClassNotifier>();
            services.AddHttpContextAccessor();
            services.AddScoped<IActivityLogger, ActivityLogger>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IInstitutionalEmailService, InstitutionalEmailService>();
            services.AddScoped<IArchiveService, ArchiveService>();
            services.AddScoped<IOnlineLessonService, OnlineLessonService>();

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(AssessmentService).Assembly);

            return services;
        }
    }
}
