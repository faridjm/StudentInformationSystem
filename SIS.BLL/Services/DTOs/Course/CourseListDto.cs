using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.Course
{
    public class CourseListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Credits { get; set; }
        public string DepartmentName { get; set; } = null!;
        public int? AcademicProgramId { get; set; }
        public string? AcademicProgramName { get; set; }
        public bool IsActive { get; set; }
    }
}
