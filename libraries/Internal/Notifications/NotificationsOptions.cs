namespace Integrations.Notifications;

public sealed class NotificationsOptions
{
    public bool Enabled { get; init; }

    public string BaseUrl { get; init; } = null!;

    public string FunctionKey { get; init; } = null!;

    public string? To { get; init; }
}
