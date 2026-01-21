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

        var tasksInRecurringProject = (await todoist.GetTasksByProjectAsync(_recurringProjectId, cancellationToken))
            .Where(task => string.IsNullOrWhiteSpace(task.ParentId))
            .ToList();

        if (tasksInRecurringProject.Count == 0)
        {
            logger.LogInformation("No tasks found.");
            return;
        }

        var labelsToUpdate = new Dictionary<string, IReadOnlyCollection<string>>();
        var tasksWithNonRecurringDueDate = new List<TodoistTask>();

        foreach (var task in tasksInRecurringProject)
        {
            ApplyRulesToTask(task, labelsToUpdate, tasksWithNonRecurringDueDate);
        }

        await ApplyLabelUpdatesAsync(tasksInRecurringProject, labelsToUpdate, cancellationToken);
        LogNumberOfTasksWithNonRecurringDueDates(tasksWithNonRecurringDueDate);
    }

    private static void ApplyRulesToTask(
        TodoistTask task,
        IDictionary<string, IReadOnlyCollection<string>> labelsToUpdate,
        List<TodoistTask> tasksWithNonRecurringDueDate)
    {
        var labels = GetLabelsSnapshot(task.Labels);
        var due = task.Due;

        if (due is null)
        {
            EnsureTaskHasOnlyInactiveLabel(task, labels, labelsToUpdate);
            return;
        }

        if (!due.IsRecurring)
        {
            tasksWithNonRecurringDueDate.Add(task);
            return;
        }

        EnsureRecurringTaskDoesNotHaveInactiveLabel(task, labels, labelsToUpdate);
    }

    private async Task ApplyLabelUpdatesAsync(
        IReadOnlyCollection<TodoistTask> tasks,
        Dictionary<string, IReadOnlyCollection<string>> labelsToUpdate,
        CancellationToken cancellationToken)
    {
        if (labelsToUpdate.Count == 0)
        {
            logger.LogInformation("No label updates required.");
            return;
        }

        var tasksToUpdate = tasks.Where(task => labelsToUpdate.ContainsKey(task.Id)).ToList();

        var updatedCount = await todoist.UpdateTasksAsync(
            tasksToUpdate,
            task => new { labels = labelsToUpdate[task.Id] },
            cancellationToken: cancellationToken);

        logger.LogInformation("Updated labels for {UpdatedCount} tasks.", updatedCount);
    }

    private static IReadOnlyCollection<string> GetLabelsSnapshot(IEnumerable<string> labels)
    {
        return labels as IReadOnlyCollection<string> ?? [..labels];
    }

    private static void EnsureTaskHasOnlyInactiveLabel(
        TodoistTask task,
        IReadOnlyCollection<string> labels,
        IDictionary<string, IReadOnlyCollection<string>> labelsToUpdate)
    {
        if (HasOnlyInactiveLabel(labels)) return;

        labelsToUpdate[task.Id] = [Constants.InactiveLabel];
    }

    private static void EnsureRecurringTaskDoesNotHaveInactiveLabel(
        TodoistTask task,
        IReadOnlyCollection<string> labels,
        IDictionary<string, IReadOnlyCollection<string>> labelsToUpdate)
    {
        if (!HasInactiveLabel(labels)) return;

        var newLabels = labels
            .Where(label => !IsInactiveLabel(label))
            .ToArray();

        labelsToUpdate[task.Id] = newLabels;
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

    private void LogNumberOfTasksWithNonRecurringDueDates(List<TodoistTask> tasksWithNonRecurringDueDate)
    {
        if (tasksWithNonRecurringDueDate.Count == 0) return;

        logger.LogWarning(
            "Found {TaskCount} tasks with non-recurring due dates in Recurring project.",
            tasksWithNonRecurringDueDate.Count);
    }
}
