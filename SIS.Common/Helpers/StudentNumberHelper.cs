namespace SIS.Common.Helpers
{
    public static class StudentNumberHelper
    {
        public static string Generate(int enrollmentYear, int sequence)
        {
            return $"{enrollmentYear}{sequence:D5}";
        }
    }
}
