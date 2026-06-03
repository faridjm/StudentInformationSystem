namespace SIS.BLL.Services.DTOs.AuditLog
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserRole { get; set; }
        public string Action { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public string? EntityId { get; set; }
        public string? Description { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuditLogFilterDto
    {
        public string? Action { get; set; }
        public string? EntityName { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
