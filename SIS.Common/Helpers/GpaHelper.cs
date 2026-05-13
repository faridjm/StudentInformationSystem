using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Common.Helpers
{
    public static class GpaHelper
    {
        public static decimal CalculateGPA(IEnumerable<(int credits, decimal score)> items)
        {
            var list = items.ToList();

            if (!list.Any()) return 0;

            decimal totalPoints = 0;
            int totalCredits = 0;

            foreach (var (credits, score) in list)
            {
                totalPoints += credits * ScoreToGradePoint(score);
                totalCredits += credits;
            }

            return totalCredits == 0 ? 0 : Math.Round(totalPoints / totalCredits, 2);
        }

        public static string ScoreToLetterGrade(decimal score)
        {
            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }

        private static decimal ScoreToGradePoint(decimal score)
        {
            return score switch
            {
                >= 90 => 4.0m,
                >= 80 => 3.0m,
                >= 70 => 2.0m,
                >= 60 => 1.0m,
                _ => 0.0m
            };
        }
    }
}
