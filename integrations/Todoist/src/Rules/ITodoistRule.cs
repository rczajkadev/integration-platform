namespace Integrations.Todoist.Rules;

internal interface ITodoistRule
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
