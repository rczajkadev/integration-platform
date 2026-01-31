using Integrations.Notifications;
using Integrations.Todoist.Rules;
using Integrations.Todoist.Rules.RecurringTasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

internal sealed class EnforceRecurringTaskRules(
    RecurringTaskInactiveLabelRule rule,
    INotificationSender notificationSender,
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
            var context = new TodoistRuleContext();
            await rule.ExecuteAsync(context, cancellationToken);
            await TrySendNotificationsAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while enforcing recurring task rules.");
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
            logger.LogError(ex, "Error while sending recurring task rules notification.");
        }
    }
}
