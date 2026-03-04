namespace Integrations.Todoist.Rules;

internal interface ITodoistRule
{
    /// <summary>
    /// Determines the execution order of the rule.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes the rule logic.
    /// </summary>
    Task ExecuteAsync(TodoistRuleContext context, CancellationToken cancellationToken);
}
