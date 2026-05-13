namespace SIS.Common.Helpers
{
    public static class EmployeeNumberHelper
    {
        public static string Generate(int year, int sequence)
        {
            return $"EMP{year}{sequence:D5}";
        }
    }
}
