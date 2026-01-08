using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.IntegrationTemplate.Functions;

internal sealed class TimerTriggerFunction(ILogger<TimerTriggerFunction> logger)
{
    [Function(nameof(TimerTriggerFunction))]
    public async Task RunAsync(
        [TimerTrigger(
            "%CronSchedule%",
            UseMonitor = false
#if DEBUG
            , RunOnStartup = true
#endif
            )] TimerInfo _,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Timer trigger function executed at: {DateTime}", DateTime.Now);

        try
        {
            await HandleFunctionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while executing function.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
