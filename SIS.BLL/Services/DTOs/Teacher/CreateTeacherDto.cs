namespace SIS.BLL.Services.DTOs.Teacher
{
    public class CreateTeacherDto
    {
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Boş buraxılsa backend @sis.az institutional email generate edir.
        /// Doldurulubsa istənilən valid email qəbul edilir (@gmail.com, @sis.az və s.)
        /// </summary>
        public string? Email { get; set; }

        public string Password { get; set; } = null!;
        public string? FinCode { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
