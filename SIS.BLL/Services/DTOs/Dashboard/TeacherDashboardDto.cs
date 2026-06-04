using SIS.BLL.Services.DTOs.Notification;

namespace SIS.BLL.Services.DTOs.Dashboard
{
    public class TeacherDashboardDto
    {
        public string TeacherName { get; set; } = "";

        // Statistik kartlar
        public int ActiveGroupsCount { get; set; }
        public int TodayClassesCount { get; set; }
        public int PendingAssignmentReviews { get; set; }
        public int ActiveQuizzesCount { get; set; }
        public int UnreadMessages { get; set; }
        public int PendingGrades { get; set; }

        // Review Queue
        public int AttendanceNotTakenCount { get; set; }
        public int SubmittedQuizResults { get; set; }

        // Siyahılar
        public List<TodayClassDto> TodayClasses { get; set; } = new();
        public List<DashboardQuizDto> ActiveQuizzes { get; set; } = new();
        public List<PendingReviewDto> PendingAssignments { get; set; } = new();
        public List<NotificationDto> RecentNotifications { get; set; } = new();
    }

    public class TodayClassDto
    {
        public int SessionId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string? Topic { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string? Room { get; set; }
        public bool AttendanceTaken { get; set; }
    }

    public class DashboardQuizDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = "";
        public string GroupName { get; set; } = "";
        public int GroupId { get; set; }
        public int AttemptCount { get; set; }
    }

    public class PendingReviewDto
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = "";
        public string GroupName { get; set; } = "";
        public int GroupId { get; set; }
        public int PendingCount { get; set; }
    }
}
