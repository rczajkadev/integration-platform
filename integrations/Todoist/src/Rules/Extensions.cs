using Microsoft.Extensions.DependencyInjection;

namespace Integrations.Todoist.Rules;

internal static class Extensions
{
    public static IServiceCollection AddTodoistRules(this IServiceCollection services)
    {
        var ruleType = typeof(ITodoistRule);

        var ruleTypes = ruleType.Assembly.GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                ruleType.IsAssignableFrom(type));

        foreach (var rule in ruleTypes)
        {
            services.AddScoped(ruleType, rule);
        }

        return services;
    }

    public static async Task ExecuteInOrderAsync(this IEnumerable<ITodoistRule> rules,
        TodoistRuleContext context,
        CancellationToken cancellationToken)
    {
        var orderedRules = rules.OrderBy(rule => rule.Order);

        foreach (var rule in orderedRules)
        {
            await rule.ExecuteAsync(context, cancellationToken);
        }
    }
}
