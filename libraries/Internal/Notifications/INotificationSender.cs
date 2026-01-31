using System.Threading;
using System.Threading.Tasks;

namespace Integrations.Notifications;

public interface INotificationSender
{
    Task SendAsync(string subject, string body, CancellationToken cancellationToken = default);
}
