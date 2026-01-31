using System.Threading;
using System.Threading.Tasks;

namespace Integrations.Notifications;

public sealed class NullNotificationSender : INotificationSender
{
    public Task SendAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
