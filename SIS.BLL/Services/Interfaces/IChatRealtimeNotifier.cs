using SIS.BLL.Services.DTOs.Chat;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Chat üçün realtime push (SignalR ChatHub-a fan-out).
    /// API layer SignalR HubContext ilə implement edir.
    /// </summary>
    public interface IChatRealtimeNotifier
    {
        Task NotifyMessageAsync(int conversationId, MessageDto message);
        Task NotifyMessagesReadAsync(int conversationId, string readerUserId);
    }
}
