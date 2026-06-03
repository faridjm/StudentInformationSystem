using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Common.Constants;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogApi _api;
        public AuditLogsController(IAuditLogApi api) { _api = api; }

        public async Task<IActionResult> Index(
            string? action, string? entityName, string? search,
            DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20)
        {
            // toDate-i həmin günün sonuna qədər əhatə et
            DateTime? toInclusive = toDate?.Date.AddDays(1).AddTicks(-1);

            var result = await _api.GetPagedAsync(action, entityName, search, fromDate, toInclusive, page, pageSize);

            try { ViewBag.Actions = (await _api.GetActionsAsync()).ToList(); }
            catch { ViewBag.Actions = new List<string>(); }
            try { ViewBag.Entities = (await _api.GetEntitiesAsync()).ToList(); }
            catch { ViewBag.Entities = new List<string>(); }

            ViewBag.Action = action;
            ViewBag.EntityName = entityName;
            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.PageSize = pageSize;

            return View(result);
        }
    }
}
