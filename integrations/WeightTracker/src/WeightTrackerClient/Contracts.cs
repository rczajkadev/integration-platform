using System.Text.Json.Serialization;

namespace Integrations.WeightTracker.WeightTrackerClient;

internal sealed record WeightsSummaryResponse(
    [property: JsonPropertyName("today")] WeightsSummaryToday Today);

internal sealed record WeightsSummaryToday(
    [property: JsonPropertyName("hasEntry")] bool HasEntry);
