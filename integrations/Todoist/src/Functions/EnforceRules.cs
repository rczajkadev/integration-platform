using Integrations.Notifications;
using Integrations.Todoist.Rules;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

internal sealed class EnforceRules(
    IEnumerable<ITodoistRule> rules,
    INotificationSender notificationSender,
    ILogger<EnforceRules> logger)
{
    [Function(nameof(EnforceRules))]
    public async Task RunAsync(
        [TimerTrigger(
            "%EnforceRulesSchedule%",
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

            foreach (var rule in rules)
                await rule.ExecuteAsync(context, cancellationToken);

            await SendNotificationsAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while enforcing Todoist rules.");
            await SendErrorNotificationsAsync(cancellationToken);
            throw;
        }
    }

    private async Task SendNotificationsAsync(TodoistRuleContext context, CancellationToken cancellationToken)
    {
        if (!context.HasMessages) return;

        const string subject = "[Integration Platform] Todoist rules: manual review required";
        var body = string.Join(Environment.NewLine, context.Messages);
        await notificationSender.SendAsync(subject, body, cancellationToken);
    }

    private async Task SendErrorNotificationsAsync(CancellationToken cancellationToken)
    {
        const string subject = "[Integration Platform] Error";
        const string body = $"Error while enforcing Todoist rules.";
        await notificationSender.SendAsync(subject, body, cancellationToken);
    }
}
