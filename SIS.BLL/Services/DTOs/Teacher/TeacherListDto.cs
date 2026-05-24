using SIS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.Teacher
{
    public class TeacherListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? FinCode { get; set; }
        public string Email { get; set; } = null!;
        public string EmployeeNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}

    