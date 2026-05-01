using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Common.Helpers
{
    public static class GpaHelper
    {
        public static decimal CalculateGPA(Dictionary<int, decimal> creditScoreMap)
        {
            if (creditScoreMap == null || creditScoreMap.Count == 0)
                return 0;

            decimal totalPoints = 0;
            int totalCredits = 0;

            foreach (var (credit, score) in creditScoreMap)
            {
                totalPoints += credit * ScoreToGradePoint(score);
                totalCredits += credit;
            }

            return totalCredits == 0 ? 0 : Math.Round(totalPoints / totalCredits, 2);
        }


        public static decimal ScoreToGradePoint(decimal score)
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

    }
}
