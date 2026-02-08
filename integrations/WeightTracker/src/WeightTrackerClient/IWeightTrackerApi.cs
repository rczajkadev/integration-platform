using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace Integrations.WeightTracker.WeightTrackerClient;

internal interface IWeightTrackerApi
{
    [Get("/api/weights/summary")]
    Task<WeightsSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
}
