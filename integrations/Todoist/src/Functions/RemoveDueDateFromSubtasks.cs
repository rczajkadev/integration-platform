using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

internal sealed class RemoveDueDateFromSubtasks(ITodoistApi todoist, ILogger<RemoveDueDateFromSubtasks> logger)
{
    [Function(nameof(RemoveDueDateFromSubtasks))]
    public async Task RunAsync(
        [TimerTrigger(
            "%RemoveDueDateFromSubtasksSchedule%",
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
            logger.LogError(ex, "Error while removing due date values.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching subtask data...");

        var todoistTasks = (await todoist.GetTasksByFilterAsync("subtask", cancellationToken))
            .Where(t => !string.IsNullOrWhiteSpace(t.ParentId)) // just to make sure
            .Where(t => t.Due is not null)
            .ToList();

        if (todoistTasks.Count == 0)
        {
            logger.LogInformation("No subtasks to update.");
            return;
        }

        logger.LogInformation("Updating subtasks...");

        var updatedCount = await RemoveDueDateAsync(todoistTasks, cancellationToken);

        logger.LogInformation("Removed due dates for {UpdatedCount} subtasks.", updatedCount);
    }

    private async Task<int> RemoveDueDateAsync(
        IEnumerable<TodoistTask> todoistTasks,
        CancellationToken cancellationToken)
    {
        var updateRequest = new { due_string = "no due date" };

        var updatedCount = await todoist.UpdateTasksAsync(
            todoistTasks,
            _ => updateRequest,
            cancellationToken: cancellationToken);

        return updatedCount;
    }
}
