using Newtonsoft.Json;

namespace Integrations.Todoist.TodoistClient;

internal interface ITodoistItem
{
    string Id { get; }
}

internal sealed record TodoistResponse<T>(
    IEnumerable<T> Results,
    string NextCursor) where T : ITodoistItem;

internal sealed record TodoistLabel(
    string Id,
    string Name) : ITodoistItem;

internal sealed record TodoistTask(
    string Id,
    string ProjectId,
    string ParentId,
    IEnumerable<string> Labels,
    TodoistTaskDue? Due,
    string Content,
    string Description) : ITodoistItem;

internal sealed record TodoistTaskDue(
    bool IsRecurring,
    string? String,
    string? Date,
    [JsonProperty(PropertyName = "datetime")] string? DateTime,
    string? Timezone);
