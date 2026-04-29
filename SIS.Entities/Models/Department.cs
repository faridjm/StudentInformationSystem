using SIS.Entities.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SIS.Entities.Models
{
    public class Department : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<AcademicProgram> Programs { get; set; } = new List<AcademicProgram>();
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();

    }
}
