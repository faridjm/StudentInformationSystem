namespace SIS.BLL.Services.DTOs.AcademicYear
{
    public class UpdateAcademicYearDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
