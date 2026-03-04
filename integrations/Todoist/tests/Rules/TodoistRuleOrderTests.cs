using Integrations.Todoist.Options;
using Integrations.Todoist.Rules;
using Integrations.Todoist.TodoistClient;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Integrations.Todoist.Tests.Rules;

public sealed class TodoistRuleOrderTests
{
    [Fact]
    public void EveryRule_ShouldHavePositiveOrder()
    {
        var rules = ResolveRules();

        Assert.NotEmpty(rules);
        Assert.All(rules, rule => Assert.True(rule.Order > 0, $"{rule.GetType().FullName} has non-positive order."));
    }

    [Fact]
    public void RuleOrders_ShouldBeUnique()
    {
        var rules = ResolveRules();

        var duplicateOrders = rules
            .GroupBy(rule => rule.Order)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} => {string.Join(", ", group.Select(rule => rule.GetType().FullName))}")
            .ToArray();

        Assert.True(
            duplicateOrders.Length == 0,
            $"Duplicate rule order numbers detected: {string.Join(" | ", duplicateOrders)}");
    }

    [Fact]
    public void RuleOrders_ShouldBeContiguous_FromOneToN()
    {
        var rules = ResolveRules().OrderBy(rule => rule.Order).ToArray();

        var actual = rules.Select(rule => rule.Order).ToArray();
        var expected = Enumerable.Range(1, rules.Length).ToArray();

        Assert.Equal(expected, actual);
    }

    private static ITodoistRule[] ResolveRules()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(
            Microsoft.Extensions.Options.Options.Create(new TodoistProjectIdsOptions
            {
                Recurring = "test-project-id"
            }));
        services.AddSingleton(Substitute.For<ITodoistApi>());
        services.AddTodoistRules();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        return [.. scope.ServiceProvider.GetServices<ITodoistRule>()];
    }
}
