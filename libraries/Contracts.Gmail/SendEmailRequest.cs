namespace Integrations.Contracts.Gmail;

public sealed record SendEmailRequest(
    string? To,
    string Subject,
    string Body,
    bool IsHtml = false);
