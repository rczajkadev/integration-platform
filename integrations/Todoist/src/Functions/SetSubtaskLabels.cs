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
            "%SubtaskLabelsCheckSchedule%",
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
        var tasks = (await todoist.GetAllTasksByFilterAsync("subtask", cancellationToken))
            .Where(t => !string.IsNullOrWhiteSpace(t.ParentId)) // just to make sure
            .Where(t => !t.Labels.Contains(Constants.SubtaskLabel) || t.Labels.Count() > 1)
            .ToList();

        if (tasks.Count == 0)
        {
            logger.LogInformation("No subtasks to update.");
            return;
        }

        var updateRequest = new TodoistUpdateTaskRequest { Labels = [Constants.SubtaskLabel] };
        var updatedCount = await todoist.UpdateTasksAsync(tasks, _ => updateRequest, cancellationToken: cancellationToken);

        logger.LogInformation("Updated labels for {UpdatedCount} subtasks.", updatedCount);
    }
}
