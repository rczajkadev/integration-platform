using Integrations.Todoist.Rules.BlockedTasks;
using Integrations.Todoist.TodoistClient;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Integrations.Todoist.Tests.Rules.BlockedTasks;

public sealed class BlockedTaskCommentRuleTests
{
    private readonly ITodoistApi _todoist = Substitute.For<ITodoistApi>();
    private readonly ILogger<BlockedTaskCommentRule> _logger = Substitute.For<ILogger<BlockedTaskCommentRule>>();

    [Fact]
    public async Task ExecuteAsync_ShouldNotReport_WhenNoBlockedTasksExist()
    {
        _todoist.GetTasksByFilterAsync($"@{Constants.BlockedLabel}", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response<TodoistTask>()));

        var rule = CreateRule();
        var context = new TodoistRuleContext();

        await rule.ExecuteAsync(context, CancellationToken.None);

        Assert.False(context.HasMessages);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReportMissingBlockerComment_WhenBlockedTaskHasNoBlockerComment()
    {
        var task = CreateTask("blocked-1", "Blocked task");

        _todoist.GetTasksByFilterAsync($"@{Constants.BlockedLabel}", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(task)));

        _todoist.GetCommentsByTaskAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response<TodoistComment>()));

        var rule = CreateRule();
        var context = new TodoistRuleContext();

        await rule.ExecuteAsync(context, CancellationToken.None);

        Assert.Single(context.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotReport_WhenBlockedTaskHasSingleValidBlockerComment()
    {
        var task = CreateTask("blocked-1", "Blocked task");

        _todoist.GetTasksByFilterAsync($"@{Constants.BlockedLabel}", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(task)));

        _todoist.GetCommentsByTaskAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(CreateComment("comment-1", task.Id, "[blocker] https://app.todoist.com/app/task/123456789"))));

        var rule = CreateRule();
        var context = new TodoistRuleContext();

        await rule.ExecuteAsync(context, CancellationToken.None);

        Assert.False(context.HasMessages);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotReport_WhenAllBlockerCommentsAreValid()
    {
        var task = CreateTask("blocked-1", "Blocked task");

        _todoist.GetTasksByFilterAsync($"@{Constants.BlockedLabel}", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(task)));

        _todoist.GetCommentsByTaskAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(
                CreateComment("comment-1", task.Id, "[blocker] https://app.todoist.com/app/task/123456789"),
                CreateComment("comment-2", task.Id, "[blocker] https://app.todoist.com/app/task/987654321"))));

        var rule = CreateRule();
        var context = new TodoistRuleContext();

        await rule.ExecuteAsync(context, CancellationToken.None);

        Assert.False(context.HasMessages);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReportInvalidBlockerUrl_WhenAnyBlockerCommentIsInvalid()
    {
        var task = CreateTask("blocked-1", "Blocked task");
        const string invalidComment = "[blocker] https://example.com/task/123";

        _todoist.GetTasksByFilterAsync($"@{Constants.BlockedLabel}", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(task)));

        _todoist.GetCommentsByTaskAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(
                CreateComment("comment-1", task.Id, "[blocker] https://app.todoist.com/app/task/123456789"),
                CreateComment("comment-2", task.Id, invalidComment))));

        var rule = CreateRule();
        var context = new TodoistRuleContext();

        await rule.ExecuteAsync(context, CancellationToken.None);

        Assert.Single(context.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReportInvalidBlockerUrl_WhenBlockerCommentUsesNonTodoistUrl()
    {
        var task = CreateTask("blocked-1", "Blocked task");
        const string invalidComment = "[blocker] https://google.com/task/123";

        _todoist.GetTasksByFilterAsync($"@{Constants.BlockedLabel}", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(task)));

        _todoist.GetCommentsByTaskAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(CreateComment("comment-1", task.Id, invalidComment))));

        var rule = CreateRule();
        var context = new TodoistRuleContext();

        await rule.ExecuteAsync(context, CancellationToken.None);

        Assert.Single(context.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIgnoreNonBlockerComments_WhenValidBlockerCommentExists()
    {
        var task = CreateTask("blocked-1", "Blocked task");

        _todoist.GetTasksByFilterAsync($"@{Constants.BlockedLabel}", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(task)));

        _todoist.GetCommentsByTaskAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response(
                CreateComment("comment-1", task.Id, "regular note"),
                CreateComment("comment-2", task.Id, "[blocker] https://app.todoist.com/app/task/123456789"))));

        var rule = CreateRule();
        var context = new TodoistRuleContext();

        await rule.ExecuteAsync(context, CancellationToken.None);

        Assert.False(context.HasMessages);
    }

    private BlockedTaskCommentRule CreateRule() => new(_todoist, _logger);

    private static TodoistResponse<T> Response<T>(params T[] items) where T : ITodoistItem =>
        new(items, string.Empty);

    private static TodoistTask CreateTask(string id, string content) =>
        new(id, "project-1", string.Empty, [Constants.BlockedLabel], null, content, string.Empty);

    private static TodoistComment CreateComment(string id, string taskId, string content) =>
        new(id, taskId, content);
}
