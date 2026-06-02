using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.ViewComponents
{
    public class UnreadChatCountViewComponent : ViewComponent
    {
        private readonly IChatApi _chatApi;

        public UnreadChatCountViewComponent(IChatApi chatApi) { _chatApi = chatApi; }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var result = await _chatApi.GetUnreadCountAsync();
                ViewBag.UnreadCount = result.Count;
                return View(result.Count);
            }
            catch
            {
                ViewBag.UnreadCount = 0;
                return View(0);
            }
        }
    }
}
