using System;
using System.Threading;
using System.Threading.Tasks;
using Integrations.Notifications;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Lotto.Functions;

internal sealed class CheckLatestResults(
    LottoClient client,
    INotificationSender notificationSender,
    ILogger<CheckLatestResults> logger)
{
    [Function(nameof(CheckLatestResults))]
    public async Task RunAsync(
        [TimerTrigger(
            "%CheckLatestResultsSchedule%",
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
            await SendExceptionNotificationAsync(ex, cancellationToken);
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching latest Lotto draw results.");

        var results = await client.GetLatestDrawResultsAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (results.DrawDateValue != today)
        {
            throw new InvalidOperationException(
                $"Expected today's Lotto results ({today:yyyy-MM-dd}), but received results for {results.DrawDate}.");
        }

        const string subject = "Lotto - latest draw results";

        var body = $"""
            Draw date: {results.DrawDate}
            Lotto: {results.LottoNumbersString}
            Lotto Plus: {results.PlusNumbersString}
            """;

        await notificationSender.SendAsync(subject, body, cancellationToken);
    }

    private async Task SendExceptionNotificationAsync(Exception exception, CancellationToken cancellationToken)
    {
        const string subject = "Lotto - failed to fetch draw results";
        await notificationSender.SendExceptionAsync(subject, exception, cancellationToken);
    }
}
