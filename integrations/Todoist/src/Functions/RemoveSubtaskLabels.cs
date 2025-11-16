using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

/// <summary>
/// Removes the 'subtask' label from all non-subtasks.
/// </summary>
internal sealed class RemoveSubtaskLabels(ITodoistApi todoist, ILogger<RemoveSubtaskLabels> logger)
{
    [Function(nameof(RemoveSubtaskLabels))]
    public async Task RunAsync(
        [TimerTrigger("%SubtaskLabelsCheckSchedule%", UseMonitor = false)] TimerInfo _,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Timer trigger function executed at: {TriggerTime}", DateTime.Now);

        try
        {
            await HandleFunctionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while removing subtask labels.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        const string query = $"!subtask & @{Constants.SubtaskLabel}";
        var tasks = (await todoist.GetAllTasksByFilterAsync(query, cancellationToken)).ToList();

        if (tasks.Count == 0)
        {
            logger.LogInformation("No tasks to update.");
            return;
        }

        var updatedCount = await todoist.UpdateTasksAsync(
            tasks,
            task => new TodoistUpdateTaskRequest
            {
                Labels = task.Labels.Where(label => label != Constants.SubtaskLabel)
            },
            cancellationToken: cancellationToken);

        logger.LogInformation("Updated labels for {UpdatedCount} tasks.", updatedCount);
    }
}
