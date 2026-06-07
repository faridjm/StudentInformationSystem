namespace SIS.Entities.Enums
{
    /// <summary>
    /// Public saytdan gələn kurs müraciətinin idarəetmə statusu.
    /// </summary>
    public enum CourseApplicationStatus
    {
        New = 0,
        Contacted = 1,
        Accepted = 2,
        Rejected = 3,
        ConvertedToStudent = 4
    }
}
