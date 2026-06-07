namespace SIS.BLL.Services.DTOs.SystemUsers
{
    /// <summary>
    /// Sistem istifadəçilər siyahısında bir sətir.
    /// PanelAccess yalnız UI üçün hesablanmış göstərici mətnidir.
    /// </summary>
    public class SystemUserListItemDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!;
        /// <summary>"Academic Admin", "PublicAdmin", "Teacher Panel", "Student Panel" və ya kombinasiya.</summary>
        public string PanelAccess { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        /// <summary>Admin rolu üçün hazırda aktiv access grant varmı? (yalnız Admin-lər üçün doludur)</summary>
        public bool? AdminAccessActive { get; set; }
    }

    public class SystemUserDetailDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!;
        public string PanelAccess { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? AdminAccessActive { get; set; }
    }

    public class CreateSystemUserDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        /// <summary>SuperAdmin / Admin / Moderator / Teacher / Student</summary>
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool SendWelcomeEmail { get; set; } = false;
    }

    public class UpdateSystemUserDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
    }

    public class ChangeSystemUserPasswordDto
    {
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class SystemUserFilterDto
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        /// <summary>"all" (default), "active", "disabled"</summary>
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
