using SIS.DAL.Context;
using SIS.DAL.Repositories.Implementations;
using SIS.DAL.Repositories.Interfaces;

namespace SIS.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IStudentRepository? _students;
        private ITeacherRepository? _teachers;
        private ICourseRepository? _courses;
        private IGroupRepository? _groups;
        private IClassSessionRepository? _classSessions;
        private IEnrollmentRepository? _enrollments;
        private IAssessmentItemRepository? _assessmentItems;
        private IAssessmentSubmissionRepository? _assessmentSubmissions;
        private IGradeRepository? _grades;
        private IAttendanceRecordRepository? _attendanceRecords;
        private IAnnouncementRepository? _announcements;
        private INotificationRepository? _notifications;
        private IActivityLogRepository? _activityLogs;
        private IAcademicYearRepository? _academicYears;
        private IRefreshTokenRepository? _refreshTokens;
        private IAssignmentRepository? _assignments;
        private IAssignmentSubmissionRepository? _assignmentSubmissionsOld;
        private IChatRepository? _chat;
        private IQuizRepository? _quizzes;
        private IQuizAttemptRepository? _quizAttempts;
        private IAuditLogRepository? _auditLogs;
        private IAdminAccessGrantRepository? _adminAccessGrants;
        private ICourseMaterialRepository? _courseMaterials;

        public UnitOfWork(AppDbContext context) { _context = context; }

        public IStudentRepository Students => _students ??= new StudentRepository(_context);
        public ITeacherRepository Teachers => _teachers ??= new TeacherRepository(_context);
        public ICourseRepository Courses => _courses ??= new CourseRepository(_context);
        public IGroupRepository Groups => _groups ??= new GroupRepository(_context);
        public IClassSessionRepository ClassSessions => _classSessions ??= new ClassSessionRepository(_context);
        public IEnrollmentRepository Enrollments => _enrollments ??= new EnrollmentRepository(_context);
        public IAssessmentItemRepository AssessmentItems => _assessmentItems ??= new AssessmentItemRepository(_context);
        public IAssessmentSubmissionRepository AssessmentSubmissions => _assessmentSubmissions ??= new AssessmentSubmissionRepository(_context);
        public IGradeRepository Grades => _grades ??= new GradeRepository(_context);
        public IAttendanceRecordRepository AttendanceRecords => _attendanceRecords ??= new AttendanceRecordRepository(_context);
        public IAnnouncementRepository Announcements => _announcements ??= new AnnouncementRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public IActivityLogRepository ActivityLogs => _activityLogs ??= new ActivityLogRepository(_context);
        public IAcademicYearRepository AcademicYears => _academicYears ??= new AcademicYearRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
        public IAssignmentRepository Assignments => _assignments ??= new AssignmentRepository(_context);
        public IAssignmentSubmissionRepository AssignmentSubmissions => _assignmentSubmissionsOld ??= new AssignmentSubmissionRepository(_context);
        public IChatRepository Chat => _chat ??= new ChatRepository(_context);
        public IQuizRepository Quizzes => _quizzes ??= new QuizRepository(_context);
        public IQuizAttemptRepository QuizAttempts => _quizAttempts ??= new QuizAttemptRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
        public IAdminAccessGrantRepository AdminAccessGrants => _adminAccessGrants ??= new AdminAccessGrantRepository(_context);
        public ICourseMaterialRepository CourseMaterials => _courseMaterials ??= new CourseMaterialRepository(_context);

        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
