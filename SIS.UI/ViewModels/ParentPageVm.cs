using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.DTOs.Parent;
using SIS.BLL.Services.DTOs.Transcript;

namespace SIS.UI.ViewModels
{
    /// <summary>Valideyn panelinin bütün read-only səhifələri üçün ümumi VM.</summary>
    public class ParentPageVm
    {
        public List<ParentChildDto> Children { get; set; } = new();
        public int SelectedStudentId { get; set; }
        public string SelectedStudentName { get; set; } = "";
        public string SelectedStudentNumber { get; set; } = "";

        public List<EnrollmentListDto> Enrollments { get; set; } = new();
        public int? SelectedEnrollmentId { get; set; }

        /// <summary>Seçilmiş enrollment üçün tam akademik snapshot (attendance, qiymət, GPA, tapşırıqlar).</summary>
        public TranscriptDto? Transcript { get; set; }

        public List<AnnouncementDto> Announcements { get; set; } = new();
        public List<ParentQuizResultDto> QuizResults { get; set; } = new();

        public bool HasChildren => Children.Count > 0;
    }
}
