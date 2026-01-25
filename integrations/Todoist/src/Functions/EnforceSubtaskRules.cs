using Integrations.Todoist.Rules.Subtasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

internal sealed class EnforceSubtaskRules(
    SubtaskLabelRule subtaskLabelRule,
    SubtaskDueDateRule subtaskDueDateRule,
    NonSubtaskLabelRule nonSubtaskLabelRule,
    ILogger<EnforceSubtaskRules> logger)
{
    [Function(nameof(EnforceSubtaskRules))]
    public async Task RunAsync(
        [TimerTrigger(
            "%EnforceSubtaskRulesSchedule%",
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
            await subtaskLabelRule.ExecuteAsync(cancellationToken);
            await subtaskDueDateRule.ExecuteAsync(cancellationToken);
            await nonSubtaskLabelRule.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while enforcing subtask rules.");
            throw;
        }
    }
}
