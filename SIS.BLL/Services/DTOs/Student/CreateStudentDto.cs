namespace SIS.BLL.Services.DTOs.Student
{
    public class CreateStudentDto
    {
        public string FullName { get; set; } = null!;

        // ── Optional valideyn hesabı (admin checkbox) ──
        public bool CreateParentAccount { get; set; } = false;
        public string? ParentFullName { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentPhone { get; set; }
        public SIS.Entities.Enums.RelationshipType ParentRelationshipType { get; set; } = SIS.Entities.Enums.RelationshipType.Guardian;
        public string? ParentPassword { get; set; }

        /// <summary>
        /// Boş buraxılsa backend @sis.az institutional email generate edir.
        /// Doldurulubsa istənilən valid email qəbul edilir (@gmail.com, @sis.az və s.)
        /// </summary>
        public string? Email { get; set; }

        public string Password { get; set; } = null!;
        public string? FinCode { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public int EnrollmentYear { get; set; }
    }
}
