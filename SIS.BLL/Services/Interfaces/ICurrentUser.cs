namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Cari HTTP context-dəki istifadəçi haqqında məlumat.
    /// Servislər bunu istifadə edərək rol əsaslı qərarlar verə bilər
    /// (məs. admin override).
    /// </summary>
    public interface ICurrentUser
    {
        string? UserId { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin();
        bool IsSuperAdmin();
        bool IsTeacher();
        bool IsStudent();

        /// <summary>Admin və ya SuperAdmin-dirmi (qaydaları bypass edə bilər).</summary>
        bool CanBypassPhaseRules => IsAdmin() || IsSuperAdmin();
    }
}
