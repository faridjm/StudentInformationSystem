using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Groups
{
    public class GroupListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
        public string? SeasonName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public bool IsActive { get; set; }

        // Tədris formatı
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.InPerson;
        public OnlinePlatform? DefaultOnlinePlatform { get; set; }
        public string? DefaultMeetingUrl { get; set; }
    }

    public class GroupDetailDto : GroupListDto
    {
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
        public List<ScheduleDayDto> ScheduleDays { get; set; } = new();
    }

    public class ScheduleDayDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
    }

    public class CreateGroupDto
    {
        public string Name { get; set; } = null!;
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
        public int? AcademicYearId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public int? CurriculumTemplateId { get; set; }
        public List<CreateScheduleDayDto> ScheduleDays { get; set; } = new();

        // Tədris formatı
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.InPerson;
        public OnlinePlatform? DefaultOnlinePlatform { get; set; }
        public string? DefaultMeetingUrl { get; set; }
    }

    public class CreateScheduleDayDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
    }

    public class UpdateGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int TeacherId { get; set; }
        public int? AcademicYearId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public List<CreateScheduleDayDto> ScheduleDays { get; set; } = new();

        // Tədris formatı
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.InPerson;
        public OnlinePlatform? DefaultOnlinePlatform { get; set; }
        public string? DefaultMeetingUrl { get; set; }
    }
}
