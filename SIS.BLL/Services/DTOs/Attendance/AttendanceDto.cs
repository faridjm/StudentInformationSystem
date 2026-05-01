using SIS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.Attendance
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public DateTime SessionDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
    }
}
