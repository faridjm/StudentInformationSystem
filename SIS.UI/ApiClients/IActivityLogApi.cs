using Refit;
using SIS.Entities.Models;

namespace SIS.UI.ApiClients;

public interface IActivityLogApi
{
    [Get("/api/activitylog")]
    Task<IEnumerable<ActivityLog>> GetRecentAsync([Query] int count = 50);
}