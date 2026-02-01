using System;
using System.Threading;
using System.Threading.Tasks;
using Integrations.Clients.Gmail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.Notifications;

internal sealed class GmailNotificationSender(
    GmailClient gmailClient,
    IOptions<NotificationsOptions> options,
    ILogger<GmailNotificationSender> logger) : INotificationSender
{
    private const string SubjectPrefix = "[Integration Platform]";

    private readonly NotificationsOptions _options = options.Value;

    public async Task SendAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return;

        try
        {
            await gmailClient.SendEmailAsync(
                _options.To,
                AddPrefixToSubject(subject),
                body,
                isHtml: false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification email.");
        }
    }

    public async Task SendExceptionAsync(string subject, Exception exception, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return;

        var body = $"""
            Operation failed at {DateTimeOffset.UtcNow:O}.
            Error: {exception.GetType().Name}: {exception.Message}";
            """;

        try
        {
            await gmailClient.SendEmailAsync(
                _options.To,
                AddPrefixToSubject(subject),
                body,
                isHtml: false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification email.");
        }
    }

    private static string AddPrefixToSubject(string subject) =>
        subject.StartsWith(SubjectPrefix, StringComparison.InvariantCultureIgnoreCase)
            ? subject
            : $"{SubjectPrefix} {subject}";
}
