using SIS.BLL.Services.DTOs.Common;
using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Public
{
    /// <summary>Public saytdan göndərilən kurs müraciəti üçün giriş modeli (anonim).</summary>
    public class CreateCourseApplicationDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public int? CourseId { get; set; }
        public int? GroupId { get; set; }
        public string? Message { get; set; }
    }

    public class CourseApplicationListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? CourseName { get; set; }
        public string? GroupName { get; set; }
        public CourseApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ConvertedStudentId { get; set; }
    }

    public class CourseApplicationDetailDto : CourseApplicationListDto
    {
        public int? CourseId { get; set; }
        public int? GroupId { get; set; }
        public string? Message { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ConvertedStudentName { get; set; }
        public string? ConvertedStudentNumber { get; set; }
    }

    public class CourseApplicationFilterDto
    {
        public string? Search { get; set; }
        public CourseApplicationStatus? Status { get; set; }
        public int? CourseId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateApplicationStatusDto
    {
        public CourseApplicationStatus Status { get; set; }
    }

    public class UpdateApplicationNoteDto
    {
        public string? AdminNote { get; set; }
    }

    /// <summary>Application → Student konversiyası üçün admin girişi.</summary>
    public class ConvertApplicationToStudentDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string Password { get; set; } = null!;
        public int EnrollmentYear { get; set; } = DateTime.UtcNow.Year;
        /// <summary>Application-da GroupId varsa və admin enroll-u istəsə.</summary>
        public bool EnrollToGroup { get; set; }
    }

    public class ConvertApplicationResultDto
    {
        public int ApplicationId { get; set; }
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = null!;
        public bool Enrolled { get; set; }
        public string? EnrollmentMessage { get; set; }
    }

    public class CourseApplicationStatsDto
    {
        public int TotalCount { get; set; }
        public int NewCount { get; set; }
        public int ContactedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int ConvertedCount { get; set; }
    }
}
