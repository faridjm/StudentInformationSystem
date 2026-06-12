using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Parent
{
    /// <summary>Student Detail → Valideynlər tab üçün.</summary>
    public class ParentInfoDto
    {
        public string ParentUserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public RelationshipType RelationshipType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Student üçün yeni valideyn hesabı yaratmaq (admin).</summary>
    public class CreateParentForStudentDto
    {
        public string FullName { get; set; } = null!;
        /// <summary>Boş buraxılsa student number əsasında auto yaradılır: parent009@sis.az</summary>
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public RelationshipType RelationshipType { get; set; } = RelationshipType.Guardian;
        /// <summary>Boş buraxılsa auto yaradılır: Parent009!</summary>
        public string? Password { get; set; }
        public bool SendWelcomeEmail { get; set; } = false; // Phase 1: istifadə olunmur
    }

    /// <summary>Mövcud valideyn user-ini student-ə bağlamaq (admin).</summary>
    public class LinkExistingParentDto
    {
        /// <summary>Valideynin email-i (və ya ParentUserId verilə bilər).</summary>
        public string? Email { get; set; }
        public string? ParentUserId { get; set; }
        public RelationshipType RelationshipType { get; set; } = RelationshipType.Guardian;
    }

    /// <summary>Yaradılan valideyn hesabının nəticəsi (admin-ə göstərmək üçün).</summary>
    public class ParentCreatedResultDto
    {
        public string ParentUserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string TemporaryPassword { get; set; } = null!;
        public string StudentName { get; set; } = "";
    }

    /// <summary>Valideyn panelində "Övladlarım" seçicisi üçün.</summary>
    public class ParentChildDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public RelationshipType RelationshipType { get; set; }
    }

    /// <summary>Valideyn üçün quiz nəticə xülasəsi (sual-sual detal yox).</summary>
    public class ParentQuizResultDto
    {
        public string QuizTitle { get; set; } = null!;
        public string GroupName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public decimal MaxScore { get; set; }
        public decimal? Score { get; set; }
        public decimal? Percentage { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
