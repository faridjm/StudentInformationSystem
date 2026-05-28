using Refit;

namespace SIS.UI.ApiClients
{
    public interface ISimulationApi
    {
        [Post("/api/simulation/set")]
        Task SetAsync([Body] SimulationSetRequest request);

        [Post("/api/simulation/clear")]
        Task ClearAsync();

        [Get("/api/simulation/status")]
        Task<SimulationStatusResponse> GetStatusAsync();
    }

    public class SimulationSetRequest
    {
        public DateTime Date { get; set; }
    }

    public class SimulationStatusResponse
    {
        public bool IsActive { get; set; }
        public string? Date { get; set; }
    }
}
