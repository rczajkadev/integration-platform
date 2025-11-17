using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.Template.Functions;

internal sealed class TimerTriggerFunction(ILogger<TimerTriggerFunction> logger)
{
    [Function(nameof(TimerTriggerFunction))]
    public void Run([TimerTrigger("%TimerTriggerSchedule%", UseMonitor = false)] TimerInfo timer)
    {
        logger.LogInformation("C# Timer trigger function executed at: {DateTime}", DateTime.Now);

        if (timer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next timer schedule at: {ScheduleStatusNext}", timer.ScheduleStatus.Next);
        }
    }
}
