using Refit;

namespace Integrations.Todoist.TodoistClient;

internal interface ITodoistApi
{
    [Get("/tasks/filter?query={query}&cursor={cursor}")]
    Task<TodoistResponse> GetTasksByFilterAsync(
        string query,
        string? cursor = null,
        CancellationToken cancellationToken = default);

    [Post("/tasks/{taskId}")]
    Task UpdateTaskAsync(
        string taskId,
        [Body] TodoistUpdateTaskRequest request,
        CancellationToken cancellationToken = default);
}
