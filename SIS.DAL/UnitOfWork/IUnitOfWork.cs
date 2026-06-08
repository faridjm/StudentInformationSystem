using SIS.DAL.Repositories.Interfaces;

namespace SIS.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        ITeacherRepository Teachers { get; }
        IDepartmentRepository Departments { get; }
        ICourseRepository Courses { get; }
        IGroupRepository Groups { get; }
        IClassSessionRepository ClassSessions { get; }
        IEnrollmentRepository Enrollments { get; }
        IAssessmentItemRepository AssessmentItems { get; }
        IAssessmentSubmissionRepository AssessmentSubmissions { get; }
        IGradeRepository Grades { get; }
        IAttendanceRecordRepository AttendanceRecords { get; }
        IAnnouncementRepository Announcements { get; }
        IAnnouncementRecipientRepository AnnouncementRecipients { get; }
        INotificationRepository Notifications { get; }
        IActivityLogRepository ActivityLogs { get; }
        IAcademicYearRepository AcademicYears { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IAssignmentRepository Assignments { get; }
        IAssignmentSubmissionRepository AssignmentSubmissions { get; }
        IChatRepository Chat { get; }
        IQuizRepository Quizzes { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IAuditLogRepository AuditLogs { get; }
        IAdminAccessGrantRepository AdminAccessGrants { get; }
        ICourseMaterialRepository CourseMaterials { get; }
        ICurriculumTemplateRepository CurriculumTemplates { get; }
        ICurriculumTemplateItemRepository CurriculumTemplateItems { get; }
        IArchiveRepository Archive { get; }
        IOnlineLessonRepository OnlineLessons { get; }
        ICourseApplicationRepository CourseApplications { get; }
        ITranscriptVerificationRepository TranscriptVerifications { get; }
        IRoomRepository Rooms { get; }

        Task<int> SaveAsync();
    }
}
