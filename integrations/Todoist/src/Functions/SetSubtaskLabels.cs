using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

/// <summary>
/// Adds a 'subtask' label to all subtasks. Removes other labels.
/// </summary>
internal sealed class SetSubtaskLabels(ITodoistApi todoist, ILogger<SetSubtaskLabels> logger)
{
    [Function(nameof(SetSubtaskLabels))]
    public async Task RunAsync(
        [TimerTrigger(
            "%SetSubtaskLabelsSchedule%",
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
            await HandleFunctionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while setting subtask labels.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching subtask data...");

        var todoistTasks = (await todoist.GetTasksByFilterAsync("subtask", cancellationToken))
            .Where(t => !string.IsNullOrWhiteSpace(t.ParentId)) // just to make sure
            .Where(t => !t.Labels.Contains(Constants.SubtaskLabel) || t.Labels.Count() > 1)
            .ToList();

        if (todoistTasks.Count == 0)
        {
            logger.LogInformation("No subtasks to update.");
            return;
        }

        logger.LogInformation("Updating subtask labels...");

        var updatedCount = await UpdateLabelsAsync(todoistTasks, cancellationToken);

        logger.LogInformation("Updating parent labels...");

        var updatedParentsCount = await UpdateParentLabelsAsync(todoistTasks, cancellationToken);

        logger.LogInformation("Updated labels for {UpdatedCount} subtasks ({UpdatedParentsCount} parents).",
            updatedCount, updatedParentsCount);
    }

    private async Task<int> UpdateLabelsAsync(
        IEnumerable<TodoistTask> todoistTasks,
        CancellationToken cancellationToken)
    {
        var updateRequest = new { labels = new[] { Constants.SubtaskLabel } };

        var updatedCount = await todoist.UpdateTasksAsync(
            todoistTasks,
            _ => updateRequest,
            cancellationToken: cancellationToken);

        return updatedCount;
    }

    private async Task<int> UpdateParentLabelsAsync(
        IList<TodoistTask> todoistTasks,
        CancellationToken cancellationToken)
    {
        var group = todoistTasks.GroupBy(t => t.ParentId).ToList();
        var parentIds = group.Select(t => t.Key).ToList();

        var parents = await todoist.GetTasksAsync(parentIds, cancellationToken);

        var updatedCount = await todoist.UpdateTasksAsync(
            parents,
            parentTask =>
            {
                var subtaskLabels = group
                    .FirstOrDefault(t => t.Key == parentTask.Id)
                    ?.SelectMany(t => t.Labels) ?? [];

                var newLabels = parentTask.Labels
                    .Concat(subtaskLabels)
                    .Distinct()
                    .Where(l => l != Constants.SubtaskLabel);

                return new { labels = newLabels };
            },
            cancellationToken: cancellationToken);

        return updatedCount;
    }
}
