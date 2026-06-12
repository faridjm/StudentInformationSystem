using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIS.DAL.Configurations;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.DAL.Context
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupScheduleDay> GroupScheduleDays { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<ClassSession> ClassSessions { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<AssessmentItem> AssessmentItems { get; set; }
        public DbSet<AssessmentSubmission> AssessmentSubmissions { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementRecipient> AnnouncementRecipients { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AdminAccessGrant> AdminAccessGrants { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }
        public DbSet<CurriculumTemplate> CurriculumTemplates { get; set; }
        public DbSet<CurriculumTemplateItem> CurriculumTemplateItems { get; set; }
        public DbSet<OnlineLesson> OnlineLessons { get; set; }
        public DbSet<OnlineLessonParticipant> OnlineLessonParticipants { get; set; }
        public DbSet<OnlineLessonChatMessage> OnlineLessonChatMessages { get; set; }
        public DbSet<CourseApplication> CourseApplications { get; set; }
        public DbSet<PublicCourseProfile> PublicCourseProfiles { get; set; }
        public DbSet<PublicTeacherProfile> PublicTeacherProfiles { get; set; }
        public DbSet<PublicFacility> PublicFacilities { get; set; }
        public DbSet<PublicGraduateStory> PublicGraduateStories { get; set; }
        public DbSet<PublicFaq> PublicFaqs { get; set; }
        public DbSet<PublicContactMessage> PublicContactMessages { get; set; }
        public DbSet<TranscriptVerification> TranscriptVerifications { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<ParentStudent> ParentStudents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }

}
