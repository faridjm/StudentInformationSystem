namespace SIS.Common.Helpers
{
    public static class StudentNumberHelper
    {
        public static string Generate(int enrollmentYear)
        {
            var random = new Random();
            var suffix = random.Next(10000, 99999);
            return $"{enrollmentYear}{suffix}";
        }
    }
}
