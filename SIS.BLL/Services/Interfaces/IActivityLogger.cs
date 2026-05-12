namespace SIS.BLL.Services.Interfaces
{
    public interface IActivityLogger
    {
        Task LogAsync(string action, string entityName, string? entityId,
                      object? oldValue, object? newValue);
    }
}
