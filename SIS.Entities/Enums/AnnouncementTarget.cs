namespace SIS.Entities.Enums
{
    public enum AnnouncementTarget
    {
        All = 0,
        Students = 1,
        Teachers = 2,

        /// <summary>Köhnə dəyər — geriyə uyğunluq üçün saxlanılır (Course/Group qrupuna bənzər).</summary>
        Course = 3,

        /// <summary>Konkret qrupun aktiv qeydiyyatdakı tələbələrinə.</summary>
        GroupStudents = 4,

        /// <summary>Admin tərəfindən fərdi seçilmiş istifadəçilərə.</summary>
        SelectedUsers = 5
    }
}
