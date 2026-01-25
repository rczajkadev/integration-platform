using Microsoft.Extensions.DependencyInjection;

namespace Integrations.Todoist.Rules;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoistRules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var ruleType = typeof(ITodoistRule);

        var ruleTypes = ruleType.Assembly.GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                ruleType.IsAssignableFrom(type));

        foreach (var type in ruleTypes)
        {
            services.AddScoped(type);
        }

        return services;
    }
}
