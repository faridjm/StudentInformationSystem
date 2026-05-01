using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.AcademicTerm
{
    public class AcademicTermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
