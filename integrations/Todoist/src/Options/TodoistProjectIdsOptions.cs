namespace Integrations.Todoist.Options;

internal sealed class TodoistProjectIdsOptions
{
    public const string SectionName = "TodoistProjectIds";

    public string NextActions { get; init; } = null!;

    public string Someday { get; init; } = null!;

    public string Recurring { get; init; } = null!;
}
