namespace Integrations.Clients.Gmail;

public sealed record SendEmailRequest(
    string? To,
    string Subject,
    string Body,
    bool IsHtml = false);
