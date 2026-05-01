using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.CourseOffering
{
    public class CourseOfferingListDto
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
        public string TermName { get; set; } = null!;
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public string? Room { get; set; }
        public bool IsActive { get; set; }
    }
}
