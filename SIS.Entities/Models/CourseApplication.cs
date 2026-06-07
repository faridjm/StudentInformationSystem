using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Public saytdan (login olmayan ziyarətçi) göndərilən kurs müraciəti.
    /// Admin paneldə idarə olunur, lazım olduqda Student-ə çevrilir.
    /// </summary>
    public class CourseApplication : BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public int? CourseId { get; set; }
        public Course? Course { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public string? Message { get; set; }

        public CourseApplicationStatus Status { get; set; } = CourseApplicationStatus.New;
        public string? AdminNote { get; set; }

        /// <summary>Müraciət student-ə çevrilibsə bu sahə dolur.</summary>
        public int? ConvertedStudentId { get; set; }
        public Student? ConvertedStudent { get; set; }
    }
}
