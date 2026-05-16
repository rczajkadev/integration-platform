using System;
using Integrations.Todoist.Options;
using Integrations.Todoist.Rules;
using Integrations.Todoist.TodoistClient;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Integrations.Todoist.Tests.Rules;

public sealed class TodoistRuleOrderTests
{
    [Fact]
    public void OrderedTypes_ShouldContainEveryRegisteredRuleExactlyOnce()
    {
        var rules = ResolveRules();

        var registeredRuleTypes = rules
            .Select(rule => rule.GetType())
            .Distinct()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        var orderedRuleTypes = TodoistRuleOrder.OrderedTypes
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(registeredRuleTypes);
        Assert.Equal(registeredRuleTypes, orderedRuleTypes);
    }

    [Fact]
    public void OrderedTypes_ShouldNotContainDuplicates()
    {
        Assert.Equal(
            TodoistRuleOrder.OrderedTypes.Count,
            TodoistRuleOrder.OrderedTypes.Distinct().Count());
    }

    [Fact]
    public void Resolve_ShouldReturnRulesInCentralOrder_WhenAllRulesAreConfigured()
    {
        var rules = ResolveRules();

        var orderedRules = TodoistRuleOrder.Resolve(rules);

        Assert.Equal(TodoistRuleOrder.OrderedTypes, orderedRules.Select(rule => rule.GetType()).ToArray());
    }

    private static ITodoistRule[] ResolveRules(ITodoistApi? todoist = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(
            Microsoft.Extensions.Options.Options.Create(new TodoistProjectIdsOptions
            {
                Recurring = "test-project-id"
            }));
        services.AddSingleton(todoist ?? Substitute.For<ITodoistApi>());
        services.AddTodoistRules();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        return [.. scope.ServiceProvider.GetServices<ITodoistRule>()];
    }

}
