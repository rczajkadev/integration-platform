using System;
using System.Threading;
using System.Threading.Tasks;

namespace Integrations.Notifications;

public sealed class NullNotificationSender : INotificationSender
{
    public Task SendAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SendExceptionAsync(string subject, Exception exception, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
