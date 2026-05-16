namespace Integrations.Todoist.Rules;

internal interface ITodoistRule
{
    /// <summary>
    /// Executes the rule logic.
    /// </summary>
    Task ExecuteAsync(TodoistRuleContext context, CancellationToken cancellationToken);
}
