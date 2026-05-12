namespace SIS.BLL.Services.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task NotifyUserAsync(string userId, string title, string message, string type);
    }
}
