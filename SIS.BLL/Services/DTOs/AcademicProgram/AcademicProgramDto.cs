using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.AcademicProgram
{
    public class AcademicProgramDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public int DurationYears { get; set; }
        public int StudentCount { get; set; }
        public bool IsActive { get; set; }
    }
}
