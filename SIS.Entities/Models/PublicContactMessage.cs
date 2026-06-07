using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>Public saytın Contact formundan göndərilən mesaj.</summary>
    public class PublicContactMessage : BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public string Message { get; set; } = null!;

        public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;
        public string? AdminNote { get; set; }
    }
}
