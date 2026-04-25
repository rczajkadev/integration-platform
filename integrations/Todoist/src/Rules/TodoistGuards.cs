using Integrations.Todoist.TodoistClient;

namespace Integrations.Todoist.Rules;

internal static class TodoistGuards
{
    public static void EnsureAllTasksAreSubtasks(
        IEnumerable<TodoistTask> tasks,
        string operationName)
    {
        EnsureTasks(
            tasks,
            task => !string.IsNullOrWhiteSpace(task.ParentId),
            operationName,
            "Expected only subtasks.");
    }

    public static void EnsureAllTasksAreTopLevel(
        IEnumerable<TodoistTask> tasks,
        string operationName)
    {
        EnsureTasks(
            tasks,
            task => string.IsNullOrWhiteSpace(task.ParentId),
            operationName,
            "Expected only top-level tasks.");
    }

    public static void EnsureAllTasksContainLabel(
        IEnumerable<TodoistTask> tasks,
        string label,
        string operationName)
    {
        EnsureTasks(
            tasks,
            task => task.Labels.Any(taskLabel => string.Equals(taskLabel, label, StringComparison.OrdinalIgnoreCase)),
            operationName,
            $"Expected every task to contain label '{label}'.");
    }

    public static void EnsureOnlyProjectTasks(
        IEnumerable<TodoistTask> tasks,
        string projectId,
        string operationName)
    {
        EnsureTasks(
            tasks,
            task => string.Equals(task.ProjectId, projectId, StringComparison.Ordinal),
            operationName,
            $"Expected every task to belong to project '{projectId}'.");
    }

    public static void EnsureOnlyExpectedTaskIds(
        IEnumerable<TodoistTask> tasks,
        IEnumerable<string> expectedTaskIds,
        string operationName)
    {
        var expectedIdSet = expectedTaskIds.ToHashSet(StringComparer.Ordinal);

        EnsureTasks(
            tasks,
            task => expectedIdSet.Contains(task.Id),
            operationName,
            "Received tasks outside the expected ID set.");
    }

    public static void EnsureNoProtectedLabelsAreDeleted(
        IEnumerable<TodoistLabel> labels,
        IEnumerable<string> protectedLabels,
        string operationName)
    {
        var protectedLabelSet = protectedLabels.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var invalidLabels = labels
            .Where(label => protectedLabelSet.Contains(label.Name))
            .ToArray();

        if (invalidLabels.Length == 0) return;

        throw new InvalidOperationException(
            $"{operationName} safety check failed. Attempted to delete protected labels: {FormatLabels(invalidLabels)}");
    }

    private static void EnsureTasks(
        IEnumerable<TodoistTask> tasks,
        Func<TodoistTask, bool> predicate,
        string operationName,
        string message)
    {
        var invalidTasks = tasks
            .Where(task => !predicate(task))
            .ToArray();

        if (invalidTasks.Length == 0) return;

        throw new InvalidOperationException(
            $"{operationName} safety check failed. {message} Offending tasks: {FormatTasks(invalidTasks)}");
    }

    private static string FormatTasks(IReadOnlyCollection<TodoistTask> tasks)
    {
        const int previewLimit = 10;

        var preview = string.Join(
            ", ",
            tasks.Take(previewLimit).Select(task => $"{task.Id} ({task.Content})"));

        return tasks.Count > previewLimit
            ? $"{preview} and {tasks.Count - previewLimit} more"
            : preview;
    }

    private static string FormatLabels(IReadOnlyCollection<TodoistLabel> labels)
    {
        return string.Join(", ", labels.Select(label => $"{label.Id} ({label.Name})"));
    }
}
