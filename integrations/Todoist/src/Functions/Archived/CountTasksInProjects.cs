using System.Text.Json;
using Integrations.Contracts.Todoist;
using Integrations.Todoist.Options;
using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.Todoist.Functions;

internal sealed class CountTasksInProjects(
    ITodoistApi todoist,
    IOptions<TodoistProjectIdsOptions> options,
    ILogger<CountTasksInProjects> logger)
{
    private readonly TodoistProjectIdsOptions _projectIds = options.Value;

    //[Function(nameof(CountTasksInProjects))]
    //[ServiceBusOutput("%NumberOfTasksServiceBusQueueName%", Connection = "ServiceBusConnectionString")]
    public async Task<string> RunAsync(
        [TimerTrigger(
            "%CountTasksInProjectsSchedule%",
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
            return await HandleFunctionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while executing function.");
            throw;
        }
    }

    private async Task<string> HandleFunctionAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching data...");

        var getProjectsTask = todoist.GetProjectsAsync(cancellationToken);
        var getTasksTask = todoist.GetTasksAsync(cancellationToken);

        var projects = (await getProjectsTask).Results.ToList();
        var tasks = (await getTasksTask).ToList();

        logger.LogInformation("Counting tasks...");

        var nextActionsProjectTasksCount = projects
            .First(p => p.Id == _projectIds.NextActions)
            .CountTasks(projects, tasks);
        var somedayProjectTasksCount = projects
            .First(p => p.Id == _projectIds.Someday)
            .CountTasks(projects, tasks);
        var recurringProjectTasksCount = projects
            .First(p => p.Id == _projectIds.Recurring)
            .CountTasks(projects, tasks);

        var contract = new NumberOfTasksInProjects(
            NextActions: nextActionsProjectTasksCount,
            Someday: somedayProjectTasksCount,
            Recurring: recurringProjectTasksCount);

        var response = JsonSerializer.Serialize(contract);

        logger.LogInformation("Returning response: {Response}", response);

        return response;
    }
}
