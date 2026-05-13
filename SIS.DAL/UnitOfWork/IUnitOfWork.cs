using SIS.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        ITeacherRepository Teachers { get; }
        ICourseRepository Courses { get; }
        ICourseOfferingRepository CourseOfferings { get; }
        IEnrollmentRepository Enrollments { get; }
        IGradeRepository Grades { get; }
        IAttendanceRecordRepository AttendanceRecords { get; }
        IAnnouncementRepository Announcements { get; }
        INotificationRepository Notifications { get; }
        IActivityLogRepository ActivityLogs { get; }
        IDepartmentRepository Departments { get; }
        IAcademicProgramRepository AcademicPrograms { get; }
        IAcademicTermRepository AcademicTerms { get; }
        IRefreshTokenRepository RefreshTokens { get; }


        Task<int> SaveAsync();

    }
}
