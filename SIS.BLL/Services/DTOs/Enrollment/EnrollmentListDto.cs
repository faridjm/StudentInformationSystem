using SIS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.Enrollment
{
    public class EnrollmentListDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
        public string TermName { get; set; } = null!;
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
