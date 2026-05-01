using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int ProgramCount { get; set; }
        public int TeacherCount { get; set; }
        public bool IsActive { get; set; }
    }
}
