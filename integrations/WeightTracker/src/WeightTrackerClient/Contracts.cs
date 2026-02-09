namespace Integrations.WeightTracker.WeightTrackerClient;

internal sealed record WeightsSummaryResponse(WeightsSummaryToday Today);

internal sealed record WeightsSummaryToday(bool HasEntry);
