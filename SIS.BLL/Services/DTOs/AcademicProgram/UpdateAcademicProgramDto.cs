namespace SIS.BLL.Services.DTOs.AcademicProgram
{
    public class UpdateAcademicProgramDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int DepartmentId { get; set; }
        public int DurationYears { get; set; }
    }
}
