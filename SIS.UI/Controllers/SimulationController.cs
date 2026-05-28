using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;
using SIS.UI.Time;

namespace SIS.UI.Controllers
{
    /// <summary>
    /// Global simulasiya rejimini idarə edir. Admin tərəfindən idarə olunur,
    /// bütün istifadəçilərə eyni anda tətbiq edilir.
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class SimulationController : Controller
    {
        private readonly ISimulationApi _simApi;
        private readonly GlobalSimulationState _state;

        public SimulationController(ISimulationApi simApi, GlobalSimulationState state)
        {
            _simApi = simApi;
            _state = state;
        }

        [HttpPost]
        public async Task<IActionResult> Set(string date, string? returnUrl)
        {
            if (DateTime.TryParse(date, out var dt))
            {
                await _simApi.SetAsync(new SimulationSetRequest { Date = dt });
                _state.Set(dt);
            }
            return Redirect(returnUrl ?? "/Admin/Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Clear(string? returnUrl)
        {
            await _simApi.ClearAsync();
            _state.Clear();
            return Redirect(returnUrl ?? "/Admin/Dashboard");
        }
    }
}
