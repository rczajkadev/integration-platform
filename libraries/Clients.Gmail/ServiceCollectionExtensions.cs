using System;
using Microsoft.Extensions.DependencyInjection;

namespace Integrations.Clients.Gmail;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGmailClient(
        this IServiceCollection services,
        string baseAddress,
        string? functionKey = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseAddress);
        return services.AddGmailClient(new Uri(baseAddress, UriKind.Absolute), functionKey);
    }

    public static IServiceCollection AddGmailClient(
        this IServiceCollection services,
        Uri baseAddress,
        string? functionKey = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(baseAddress);

        services.AddHttpClient<GmailClient>(client =>
        {
            client.BaseAddress = baseAddress;
            client.DefaultRequestHeaders.Add(Defaults.FunctionKeyHeaderName, functionKey);
        });

        return services;
    }
}
