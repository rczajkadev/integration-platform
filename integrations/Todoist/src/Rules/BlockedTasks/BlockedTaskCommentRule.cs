using Integrations.Todoist.TodoistClient;
using Microsoft.Extensions.Logging;

namespace Integrations.Todoist.Rules.BlockedTasks;

/// <summary>
/// Validates that blocked tasks have blocker comments pointing to Todoist task URLs.
/// </summary>
internal sealed class BlockedTaskCommentRule(
    ITodoistApi todoist,
    ILogger<BlockedTaskCommentRule> logger) : ITodoistRule
{
    private const string BlockedFilter = "@blocked";
    private const string BlockerCommentPrefix = "[blocker]";
    private const string TodoistTaskPathPrefix = "/app/task/";

    public int Order => 6;

    /// <inheritdoc />
    /// <seealso cref="BlockedTaskCommentRule" />
    public async Task ExecuteAsync(TodoistRuleContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching tasks with the {Label} label...", Constants.BlockedLabel);
        var blockedTasks = (await todoist.GetTasksByFilterAsync(BlockedFilter, cancellationToken)).ToArray();

        if (blockedTasks.Length == 0)
        {
            logger.LogInformation("No blocked tasks found.");
            return;
        }

        var validationResults = await Task.WhenAll(blockedTasks.Select(task => ValidateCommentsAsync(task, cancellationToken)));

        var invalidTasks = validationResults
            .OfType<ValidationError>()
            .OrderBy(result => result.Task.Content, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (invalidTasks.Length == 0)
        {
            logger.LogInformation("All blocked tasks contain valid blocker comments.");
            return;
        }

        logger.LogWarning("Found {TaskCount} blocked tasks with invalid blocker comments.", invalidTasks.Length);
        context.AddMessage(BuildMessage(invalidTasks));
    }

    private async Task<ValidationError?> ValidateCommentsAsync(TodoistTask task, CancellationToken cancellationToken)
    {
        var blockerComments = (await todoist.GetCommentsByTaskAsync(task.Id, cancellationToken)).Results
            .Select(comment => comment.Content.Trim())
            .Where(comment => comment.StartsWith(BlockerCommentPrefix, StringComparison.Ordinal))
            .ToArray();

        if (blockerComments.Length == 0)
            return new ValidationError(task, "missing blocker comment");

        foreach (var blockerComment in blockerComments)
        {
            if (!IsValidBlockerComment(blockerComment))
                return new ValidationError(task, $"invalid blocker URL: {blockerComment}");
        }

        return null;
    }

    private static bool IsValidBlockerComment(string blockerComment)
    {
        if (!blockerComment.StartsWith(BlockerCommentPrefix, StringComparison.Ordinal))
            return false;

        var url = blockerComment[BlockerCommentPrefix.Length..].Trim();

        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl))
            return false;

        if (!string.Equals(parsedUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.Equals(parsedUrl.Host, "app.todoist.com", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrEmpty(parsedUrl.Query) || !string.IsNullOrEmpty(parsedUrl.Fragment))
            return false;

        if (!parsedUrl.AbsolutePath.StartsWith(TodoistTaskPathPrefix, StringComparison.Ordinal))
            return false;

        var taskId = parsedUrl.AbsolutePath[TodoistTaskPathPrefix.Length..];
        return !string.IsNullOrWhiteSpace(taskId) && !taskId.Contains('/');
    }

    private static string BuildMessage(IReadOnlyCollection<ValidationError> invalidTasks)
    {
        var items = invalidTasks.Select((task, index) => $"{index + 1}) {task.Task.Content} - {task.Message}");

        return $"Found {invalidTasks.Count} blocked tasks with invalid blocker comments:{Environment.NewLine}" +
               string.Join(Environment.NewLine, items);
    }

    private sealed record ValidationError(TodoistTask Task, string Message);
}
