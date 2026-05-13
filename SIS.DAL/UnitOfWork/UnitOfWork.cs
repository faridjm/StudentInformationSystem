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
        private ICourseOfferingRepository? _courseOfferings;
        private IEnrollmentRepository? _enrollments;
        private IGradeRepository? _grades;
        private IAttendanceRecordRepository? _attendanceRecords;
        private IAnnouncementRepository? _announcements;
        private INotificationRepository? _notifications;
        private IActivityLogRepository? _activityLogs;
        private IDepartmentRepository? _departments;
        private IAcademicProgramRepository? _academicPrograms;
        private IAcademicTermRepository? _academicTerms;
        private IRefreshTokenRepository? _refreshTokens;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IStudentRepository Students
            => _students ??= new StudentRepository(_context);

        public ITeacherRepository Teachers
            => _teachers ??= new TeacherRepository(_context);

        public ICourseRepository Courses
            => _courses ??= new CourseRepository(_context);

        public ICourseOfferingRepository CourseOfferings
            => _courseOfferings ??= new CourseOfferingRepository(_context);

        public IEnrollmentRepository Enrollments
            => _enrollments ??= new EnrollmentRepository(_context);

        public IGradeRepository Grades
            => _grades ??= new GradeRepository(_context);

        public IAttendanceRecordRepository AttendanceRecords
            => _attendanceRecords ??= new AttendanceRecordRepository(_context);

        public IAnnouncementRepository Announcements
            => _announcements ??= new AnnouncementRepository(_context);

        public INotificationRepository Notifications
            => _notifications ??= new NotificationRepository(_context);

        public IActivityLogRepository ActivityLogs
            => _activityLogs ??= new ActivityLogRepository(_context);

        public IDepartmentRepository Departments
            => _departments ??= new DepartmentRepository(_context);

        public IAcademicProgramRepository AcademicPrograms
            => _academicPrograms ??= new AcademicProgramRepository(_context);

        public IAcademicTermRepository AcademicTerms
    => _academicTerms ??= new AcademicTermRepository(_context);

        public IRefreshTokenRepository RefreshTokens
    => _refreshTokens ??= new RefreshTokenRepository(_context);

        public async Task<int> SaveAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();

    }
}
