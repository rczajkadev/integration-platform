using Integrations.Todoist.Options;
using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.Todoist.Functions;

internal sealed class EnforceRecurringTaskRules(
    ITodoistApi todoist,
    IOptions<TodoistProjectIdsOptions> options,
    ILogger<EnforceRecurringTaskRules> logger)
{
    private readonly string _recurringProjectId = options.Value.Recurring;

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
            await HandleFunctionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while enforcing recurring task rules.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_recurringProjectId))
        {
            logger.LogWarning("Recurring project ID is not configured.");
            return;
        }

        logger.LogInformation("Fetching tasks from Recurring project...");

        var tasks = (await todoist.GetTasksByProjectAsync(_recurringProjectId, cancellationToken)).ToList();

        if (tasks.Count == 0)
        {
            logger.LogInformation("No tasks found.");
            return;
        }

        var updates = new Dictionary<string, IReadOnlyCollection<string>>();
        var nonRecurringDueTasks = new List<TodoistTask>();
        var list = new List<string>();

        foreach (var task in tasks)
        {
            list.AddRange(task.Labels);

            var labels = task.Labels as IReadOnlyCollection<string> ?? list;
            var due = task.Due;

            if (due is null)
            {
                if (!HasOnlyInactiveLabel(labels))
                {
                    updates[task.Id] = [Constants.InactiveLabel];
                }

                continue;
            }

            if (!due.IsRecurring)
            {
                nonRecurringDueTasks.Add(task);
                continue;
            }

            if (HasInactiveLabel(labels))
            {
                var newLabels = labels
                    .Where(label => !IsInactiveLabel(label))
                    .ToArray();

                updates[task.Id] = newLabels;
            }
        }

        if (updates.Count > 0)
        {
            var tasksToUpdate = tasks.Where(task => updates.ContainsKey(task.Id)).ToList();

            var updatedCount = await todoist.UpdateTasksAsync(
                tasksToUpdate,
                task => new { labels = updates[task.Id] },
                cancellationToken: cancellationToken);

            logger.LogInformation("Updated labels for {UpdatedCount} tasks.", updatedCount);
        }
        else
        {
            logger.LogInformation("No label updates required.");
        }

        if (nonRecurringDueTasks.Count > 0)
        {
            logger.LogWarning(
                "Found {TaskCount} tasks with non-recurring due dates in Recurring project.",
                nonRecurringDueTasks.Count);
        }
    }

    private static bool HasOnlyInactiveLabel(IReadOnlyCollection<string> labels)
    {
        return labels.Count == 1 && HasInactiveLabel(labels);
    }

    private static bool HasInactiveLabel(IEnumerable<string> labels)
    {
        return labels.Any(IsInactiveLabel);
    }

    private static bool IsInactiveLabel(string label)
    {
        return string.Equals(label, Constants.InactiveLabel, StringComparison.OrdinalIgnoreCase);
    }
}
