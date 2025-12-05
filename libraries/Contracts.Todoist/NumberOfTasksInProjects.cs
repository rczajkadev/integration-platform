namespace Integrations.Contracts.Todoist;

public sealed record NumberOfTasksInProjects(
    int NextActions,
    int Someday,
    int Recurring);
