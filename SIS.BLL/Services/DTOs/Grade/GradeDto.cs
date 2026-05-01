using SIS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.Grade
{
    public class GradeDto
    { 
        public int Id { get; set; }
        public GradeType Type { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Note { get; set; }
        public DateTime GradedAt { get; set; }
        public string LetterGrade { get; set; } = null!;
    }
}
