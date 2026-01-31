using System;
using Integrations.Clients.Gmail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integrations.Notifications;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifications(
        this IServiceCollection services,
        IConfiguration configuration,
        string optionsSectionName)
    {
        ArgumentException.ThrowIfNullOrEmpty(optionsSectionName);

        var section = configuration.GetSection(optionsSectionName);
        services.Configure<NotificationsOptions>(section);

        var enabled = section.GetValue<bool>("Enabled");

        if (!enabled)
        {
            services.AddSingleton<INotificationSender, NullNotificationSender>();
            return services;
        }

        const string baseUrlKey = "BaseUrl";
        const string functionKeyKey = "FunctionKey";

        var baseUrl = section[baseUrlKey];
        var functionKey = section[functionKeyKey];

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException($"{optionsSectionName}:{baseUrlKey} is not configured.");

        if (string.IsNullOrWhiteSpace(functionKey))
            throw new InvalidOperationException($"{optionsSectionName}:{functionKeyKey} is not configured.");

        services.AddGmailClient(baseUrl, functionKey);
        services.AddSingleton<INotificationSender, GmailNotificationSender>();
        services.AddLogging();

        return services;
    }
}
