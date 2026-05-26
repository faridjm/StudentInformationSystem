namespace SIS.BLL.Services.DTOs.AcademicYear
{
    public class CreateAcademicYearDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
