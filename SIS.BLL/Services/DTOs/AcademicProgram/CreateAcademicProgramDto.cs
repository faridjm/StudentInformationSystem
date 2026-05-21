namespace SIS.BLL.Services.DTOs.AcademicProgram
{
    public class CreateAcademicProgramDto
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int DepartmentId { get; set; }
        public int DurationYears { get; set; }
        public string? Description { get; set; }
    }
}
