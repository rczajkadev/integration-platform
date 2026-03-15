using Refit;

namespace Integrations.Todoist.TodoistClient;

internal interface ITodoistApi
{
    [Get("/labels?limit=200")]
    Task<TodoistResponse<TodoistLabel>> GetLabelsAsync(
        CancellationToken cancellationToken = default);

    [Get("/tasks?ids={ids}&cursor={cursor}")]
    Task<TodoistResponse<TodoistTask>> GetTasksAsync(
        string ids,
        string? cursor = null,
        CancellationToken cancellationToken = default);

    [Get("/tasks?cursor={cursor}")]
    Task<TodoistResponse<TodoistTask>> GetTasksAsync(
        string? cursor = null,
        CancellationToken cancellationToken = default);

    [Get("/tasks?project_id={projectId}&cursor={cursor}")]
    Task<TodoistResponse<TodoistTask>> GetTasksByProjectAsync(
        string projectId,
        string? cursor = null,
        CancellationToken cancellationToken = default);

    [Get("/tasks/filter?query={query}&cursor={cursor}")]
    Task<TodoistResponse<TodoistTask>> GetTasksByFilterAsync(
        string query,
        string? cursor = null,
        CancellationToken cancellationToken = default);

    [Get("/comments?task_id={taskId}&limit=200")]
    Task<TodoistResponse<TodoistComment>> GetCommentsByTaskAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    [Post("/tasks/{taskId}")]
    Task UpdateTaskAsync(
        string taskId,
        [Body] object request,
        CancellationToken cancellationToken = default);

    [Delete("/labels/{label_id}")]
    Task DeleteLabelAsync(
        [AliasAs("label_id")] string labelId,
        CancellationToken cancellationToken = default);
}
