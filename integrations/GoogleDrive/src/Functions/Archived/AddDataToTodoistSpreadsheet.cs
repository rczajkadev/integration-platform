using Integrations.Contracts.Todoist;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.GoogleDrive.Functions;

internal sealed class AddDataToTodoistSpreadsheet(ILogger<AddDataToTodoistSpreadsheet> logger)
{
    //[Function(nameof(AddDataToTodoistSpreadsheet))]
    public async Task Run(
        [ServiceBusTrigger("%NumberOfTasksServiceBusQueueName%", Connection = "ServiceBusConnectionString")]
        NumberOfTasksInProjects message,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Service Bus Queue trigger function executed at: {DateTime}", DateTime.Now);

        try
        {
            await HandleFunctionAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while executing function.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(NumberOfTasksInProjects _, CancellationToken __)
    {
        await Task.CompletedTask;
    }
}
