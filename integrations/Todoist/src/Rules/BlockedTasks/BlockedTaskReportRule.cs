using Integrations.Todoist.TodoistClient;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Rules.BlockedTasks;

/// <summary>
/// Reports tasks that are currently marked as blocked.
/// </summary>
internal sealed class BlockedTaskReportRule(
    ITodoistApi todoist,
    ILogger<BlockedTaskReportRule> logger) : ITodoistRule
{
    private const string BlockedFilter = "@blocked";

    public int Order => 7;

    /// <inheritdoc />
    /// <seealso cref="BlockedTaskReportRule" />
    public async Task ExecuteAsync(TodoistRuleContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching tasks with the {Label} label for blocked task report...", Constants.BlockedLabel);

        var blockedTasks = (await todoist.GetTasksByFilterAsync(BlockedFilter, cancellationToken))
            .OrderBy(task => task.Content, StringComparer.OrdinalIgnoreCase)
            .ThenBy(task => task.Id, StringComparer.Ordinal)
            .ToArray();

        if (blockedTasks.Length == 0)
        {
            logger.LogInformation("No blocked tasks found for report.");
            return;
        }

        logger.LogInformation("Found {TaskCount} blocked tasks.", blockedTasks.Length);
        context.AddMessage(NotificationFormatter.BuildNumberedListMessage(
            $"Found {blockedTasks.Length} blocked tasks:",
            blockedTasks.Select(task => task.Content)));
    }
}
