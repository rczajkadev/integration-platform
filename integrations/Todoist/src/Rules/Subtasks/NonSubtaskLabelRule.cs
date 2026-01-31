using Integrations.Todoist.TodoistClient;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Rules.Subtasks;

/// <summary>
/// Removes the subtask label from tasks that are not subtasks but have it assigned.
/// </summary>
internal sealed class NonSubtaskLabelRule(
    ITodoistApi todoist,
    ILogger<NonSubtaskLabelRule> logger) : ITodoistRule
{
    private const string NonSubtaskWithSubtaskLabelFilter = "!subtask & @subtask";

    /// <inheritdoc />
    /// <seealso cref="NonSubtaskLabelRule" />
    public async Task ExecuteAsync(TodoistRuleContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching non subtasks but with subtask label...");
        var tasks = await FetchNonSubtasksWithSubtaskLabelAsync(cancellationToken);

        if (tasks.Count == 0)
        {
            logger.LogInformation("No non-subtasks with subtask label to update.");
            return;
        }

        logger.LogInformation("Removing subtask label from non-subtasks...");

        var updatedCount = await todoist.UpdateTasksAsync(
            tasks,
            task => new
            {
                labels = task.Labels.Where(label => !IsSubtaskLabel(label)).ToArray()
            },
            cancellationToken: cancellationToken);

        logger.LogInformation("Updated labels for {UpdatedCount} non-subtasks.", updatedCount);
    }

    private async Task<List<TodoistTask>> FetchNonSubtasksWithSubtaskLabelAsync(CancellationToken cancellationToken)
    {
        return [.. await todoist.GetTasksByFilterAsync(NonSubtaskWithSubtaskLabelFilter, cancellationToken)];
    }

    private static bool IsSubtaskLabel(string label)
    {
        return string.Equals(label, Constants.SubtaskLabel, StringComparison.Ordinal);
    }
}
