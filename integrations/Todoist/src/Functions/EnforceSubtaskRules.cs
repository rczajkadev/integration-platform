using Integrations.Notifications;
using Integrations.Todoist.Rules;
using Integrations.Todoist.Rules.Subtasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

internal sealed class EnforceSubtaskRules(
    SubtaskLabelRule subtaskLabelRule,
    SubtaskDueDateRule subtaskDueDateRule,
    NonSubtaskLabelRule nonSubtaskLabelRule,
    INotificationSender notificationSender,
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
            var context = new TodoistRuleContext();
            await subtaskLabelRule.ExecuteAsync(context, cancellationToken);
            await subtaskDueDateRule.ExecuteAsync(context, cancellationToken);
            await nonSubtaskLabelRule.ExecuteAsync(context, cancellationToken);
            await TrySendNotificationsAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while enforcing subtask rules.");
            throw;
        }
    }

    private async Task TrySendNotificationsAsync(
        TodoistRuleContext context,
        CancellationToken cancellationToken)
    {
        if (!context.HasMessages) return;

        const string subject = "Todoist rules: manual review required";
        var body = string.Join(Environment.NewLine, context.Messages);

        try
        {
            await notificationSender.SendAsync(subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while sending subtask rules notification.");
        }
    }
}
