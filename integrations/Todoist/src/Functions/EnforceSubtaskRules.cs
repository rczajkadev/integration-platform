using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Functions;

internal sealed class EnforceSubtaskRules(ITodoistApi todoist, ILogger<EnforceSubtaskRules> logger)
{
    private const string RemoveDueDateValue = "no due date";
    private const string SubtaskFilter = "subtask";
    private const string NonSubtaskWithSubtaskLabelFilter = "!subtask & @subtask";

    [Function(nameof(EnforceSubtaskRules))]
    public async Task RunAsync(
        [TimerTrigger(
            "%EnforceSubtaskRulesSchedule%",
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
            logger.LogError(ex, "Error while enforcing subtask rules.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching subtasks...");
        var subtasks = await FetchSubtasksAsync(cancellationToken);

        logger.LogInformation("Fetching non subtasks but with subtask label...");
        var nonSubtasksWithSubtaskLabel = await FetchNonSubtasksWithSubtaskLabelAsync(cancellationToken);

        if (subtasks.Count == 0 && nonSubtasksWithSubtaskLabel.Count == 0)
        {
            logger.LogInformation("No tasks to update.");
            return;
        }

        var updatePlan = BuildUpdatePlan(subtasks);

        await ApplySubtaskUpdatesAsync(subtasks, updatePlan.SubtaskUpdates, cancellationToken);
        await UpdateParentLabelsAsync(updatePlan.ParentLabelsUpdates, cancellationToken);
        await RemoveSubtaskLabelFromNonSubtasksAsync(nonSubtasksWithSubtaskLabel, cancellationToken);
    }

    private async Task<List<TodoistTask>> FetchSubtasksAsync(CancellationToken cancellationToken)
    {
        return [..(await todoist.GetTasksByFilterAsync(SubtaskFilter, cancellationToken))
            .Where(task => !string.IsNullOrWhiteSpace(task.ParentId))];
    }

    private async Task<List<TodoistTask>> FetchNonSubtasksWithSubtaskLabelAsync(CancellationToken cancellationToken)
    {
        return [..await todoist.GetTasksByFilterAsync(NonSubtaskWithSubtaskLabelFilter, cancellationToken)];
    }

    private static SubtaskUpdatePlan BuildUpdatePlan(IReadOnlyCollection<TodoistTask> subtasks)
    {
        var subtaskUpdates = new Dictionary<string, SubtaskUpdate>();
        var parentLabelsToAdd = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var task in subtasks)
        {
            var labels = GetLabelsSnapshot(task.Labels);
            var shouldResetLabels = !HasOnlySubtaskLabel(labels);
            var shouldRemoveDueDate = task.Due is not null;

            if (shouldResetLabels || shouldRemoveDueDate)
            {
                var labelsForUpdate = shouldResetLabels
                    ? new[] { Constants.SubtaskLabel }
                    : labels;

                subtaskUpdates[task.Id] = new SubtaskUpdate(labelsForUpdate, shouldRemoveDueDate);
            }

            if (shouldResetLabels) AddLabelsToParent(parentLabelsToAdd, task.ParentId, labels);
        }

        return new SubtaskUpdatePlan(subtaskUpdates, parentLabelsToAdd);
    }

    private async Task ApplySubtaskUpdatesAsync(
        IReadOnlyCollection<TodoistTask> subtasks,
        IReadOnlyDictionary<string, SubtaskUpdate> subtaskUpdates,
        CancellationToken cancellationToken)
    {
        if (subtaskUpdates.Count == 0)
        {
            logger.LogInformation("No subtasks to update.");
            return;
        }

        logger.LogInformation("Updating subtasks...");

        var tasksToUpdate = subtasks.Where(task => subtaskUpdates.ContainsKey(task.Id)).ToList();

        var updatedCount = await todoist.UpdateTasksAsync(
            tasksToUpdate,
            task => CreateSubtaskUpdateRequest(subtaskUpdates[task.Id]),
            cancellationToken: cancellationToken);

        logger.LogInformation("Updated {UpdatedCount} subtasks.", updatedCount);
    }

    private async Task UpdateParentLabelsAsync(
        IReadOnlyDictionary<string, HashSet<string>> parentLabelsToAdd,
        CancellationToken cancellationToken)
    {
        if (parentLabelsToAdd.Count == 0)
        {
            logger.LogInformation("No parent labels to update.");
            return;
        }

        logger.LogInformation("Updating parent labels...");

        var parents = await todoist.GetTasksAsync(parentLabelsToAdd.Keys.ToList(), cancellationToken);

        var updatedCount = await todoist.UpdateTasksAsync(
            parents,
            parentTask =>
            {
                if (!parentLabelsToAdd.TryGetValue(parentTask.Id, out var labelsToAdd))
                    return new { labels = parentTask.Labels.ToArray() };

                var newLabels = MergeParentLabels(parentTask.Labels, labelsToAdd);
                return new { labels = newLabels };
            },
            cancellationToken: cancellationToken);

        logger.LogInformation("Updated labels for {UpdatedCount} parents.", updatedCount);
    }

    private async Task RemoveSubtaskLabelFromNonSubtasksAsync(
        IReadOnlyCollection<TodoistTask> tasks,
        CancellationToken cancellationToken)
    {
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

    private static void AddLabelsToParent(
        IDictionary<string, HashSet<string>> parentLabelsToAdd,
        string parentId,
        IReadOnlyCollection<string> labels)
    {
        if (string.IsNullOrWhiteSpace(parentId)) return;

        if (!parentLabelsToAdd.TryGetValue(parentId, out var existingLabels))
        {
            existingLabels = new HashSet<string>(StringComparer.Ordinal);
            parentLabelsToAdd[parentId] = existingLabels;
        }

        foreach (var label in labels.Where(label => !IsSubtaskLabel(label)))
        {
            existingLabels.Add(label);
        }
    }

    private static object CreateSubtaskUpdateRequest(SubtaskUpdate update)
    {
        return update.RemoveDueDate
            ? new { labels = update.Labels, due_string = RemoveDueDateValue }
            : new { labels = update.Labels };
    }

    private static string[] MergeParentLabels(
        IEnumerable<string> parentLabels,
        IEnumerable<string> subtaskLabels)
    {
        return [..parentLabels
            .Concat(subtaskLabels)
            .Distinct()
            .Where(label => !IsSubtaskLabel(label))];
    }

    private static IReadOnlyCollection<string> GetLabelsSnapshot(IEnumerable<string> labels)
    {
        return labels as IReadOnlyCollection<string> ?? [..labels];
    }

    private static bool HasOnlySubtaskLabel(IReadOnlyCollection<string> labels)
    {
        return labels.Count == 1 && labels.Any(IsSubtaskLabel);
    }

    private static bool IsSubtaskLabel(string label)
    {
        return string.Equals(label, Constants.SubtaskLabel, StringComparison.Ordinal);
    }

    private sealed record SubtaskUpdate(IReadOnlyCollection<string> Labels, bool RemoveDueDate);

    private sealed record SubtaskUpdatePlan(
        IReadOnlyDictionary<string, SubtaskUpdate> SubtaskUpdates,
        IReadOnlyDictionary<string, HashSet<string>> ParentLabelsUpdates);
}
