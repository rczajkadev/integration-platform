using Newtonsoft.Json;
using Refit;

namespace Integrations.Todoist;

internal interface ITodoistApi
{
    [Get("/tasks/filter?query={query}&cursor={cursor}")]
    Task<TodoistResponse> GetTasksByFilterAsync(string query, string? cursor = null);

    [Post("/tasks/{taskId}")]
    Task UpdateTaskAsync(string taskId, [Body] TodoistUpdateTaskRequest request);
}

internal sealed record TodoistResponse(
    IEnumerable<TodoistTask> Results,
    [JsonProperty(PropertyName="next_cursor")] string NextCursor);

internal sealed record TodoistTask(
    string Id,
    [JsonProperty(PropertyName="parent_id")] string ParentId,
    IEnumerable<string> Labels,
    string Content,
    string Description);

internal sealed class TodoistUpdateTaskRequest
{
    [JsonProperty(PropertyName = "labels")]
    public IEnumerable<string>? Labels { get; set; }
}
