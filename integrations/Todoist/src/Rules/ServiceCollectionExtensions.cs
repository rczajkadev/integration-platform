using Microsoft.Extensions.DependencyInjection;

namespace Integrations.Todoist.Rules;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoistRules(this IServiceCollection services)
    {
        var ruleType = typeof(ITodoistRule);

        var ruleTypes = ruleType.Assembly.GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                ruleType.IsAssignableFrom(type))
            .OrderBy(type => type.FullName);

        foreach (var type in ruleTypes)
        {
            services.AddScoped(ruleType, type);
        }

        return services;
    }
}
