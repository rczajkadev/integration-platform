using Integrations.Todoist.Options;
using Integrations.Todoist.TodoistClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.Todoist.Rules.RecurringTasks;

/// <summary>
/// Reports inactive tasks in the Recurring project.
/// </summary>
internal sealed class RecurringTaskInactiveReportRule(
    ITodoistApi todoist,
    IOptions<TodoistProjectIdsOptions> options,
    ILogger<RecurringTaskInactiveReportRule> logger) : ITodoistRule
{
    public int Order => 5;

    private readonly string _recurringProjectId = options.Value.Recurring;

    /// <inheritdoc />
    /// <seealso cref="RecurringTaskInactiveReportRule" />
    public async Task ExecuteAsync(TodoistRuleContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_recurringProjectId))
        {
            logger.LogWarning("Recurring project ID is not configured.");
            return;
        }

        logger.LogInformation("Fetching tasks from Recurring project for inactive report...");

        var inactiveTasks = (await todoist.GetTasksByProjectAsync(_recurringProjectId, cancellationToken))
            .Where(task => string.IsNullOrWhiteSpace(task.ParentId))
            .Where(task => task.Labels.Any(IsInactiveLabel))
            .OrderBy(task => task.Content, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (inactiveTasks.Length == 0)
        {
            logger.LogInformation("No inactive tasks found in Recurring project.");
            return;
        }

        logger.LogInformation("Found {TaskCount} inactive tasks in Recurring project.", inactiveTasks.Length);
        context.AddMessage(BuildInactiveTasksMessage(inactiveTasks));
    }

    private static string BuildInactiveTasksMessage(IReadOnlyCollection<TodoistTask> inactiveTasks)
    {
        var items = inactiveTasks
            .Select((task, index) => $"{index + 1}) {task.Content} ({task.Id})");

        return $"""
            Found {inactiveTasks.Count} inactive tasks in Recurring project:
            {string.Join(Environment.NewLine, items)}
            """;
    }

    private static bool IsInactiveLabel(string label)
    {
        return string.Equals(label, Constants.InactiveLabel, StringComparison.OrdinalIgnoreCase);
    }
}
