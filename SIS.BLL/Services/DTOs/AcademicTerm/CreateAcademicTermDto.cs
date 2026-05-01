namespace SIS.BLL.Services.DTOs.AcademicTerm
{
    public class CreateAcademicTermDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
