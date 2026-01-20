using Refit;

namespace Integrations.Todoist.TodoistClient;

internal interface ITodoistApi
{
    [Get("/projects?limit=200")]
    Task<TodoistResponse<TodoistProject>> GetProjectsAsync(
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

    [Post("/tasks/{taskId}")]
    Task UpdateTaskAsync(
        string taskId,
        [Body] object request,
        CancellationToken cancellationToken = default);
}
