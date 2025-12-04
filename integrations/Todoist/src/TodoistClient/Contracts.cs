using Newtonsoft.Json;

namespace Integrations.Todoist.TodoistClient;

internal interface ITodoistItem
{
    string Id { get; }
}

internal sealed record TodoistResponse<T>(
    IEnumerable<T> Results,
    [JsonProperty(PropertyName = "next_cursor")] string NextCursor) where T : ITodoistItem;

internal sealed record TodoistProject(
    string Id,
    [JsonProperty(PropertyName = "parent_id")] string ParentId,
    string Name,
    string Description) : ITodoistItem;

internal sealed record TodoistTask(
    string Id,
    [JsonProperty(PropertyName = "project_id")] string ProjectId,
    [JsonProperty(PropertyName = "parent_id")] string ParentId,
    IEnumerable<string> Labels,
    object? Due, // TODO: specify type if needed
    string Content,
    string Description) : ITodoistItem;
