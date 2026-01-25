using Integrations.Todoist.Rules.RecurringTasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

internal sealed class EnforceRecurringTaskRules(
    RecurringTaskInactiveLabelRule rule,
    ILogger<EnforceRecurringTaskRules> logger)
{
    [Function(nameof(EnforceRecurringTaskRules))]
    public async Task RunAsync(
        [TimerTrigger(
            "%EnforceRecurringTaskRulesSchedule%",
            UseMonitor = false
#if DEBUG
            , RunOnStartup = true
#endif
        )] TimerInfo _,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Timer trigger function executed at: {TriggerTime}", DateTime.Now);

        try
        {
            await rule.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while enforcing recurring task rules.");
            throw;
        }
    }
}
