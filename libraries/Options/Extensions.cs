using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Integrations.Options;

public static class Extensions
{
    private static readonly ConcurrentDictionary<Type, object> Cache = new();

    public static TOptions GetOptions<TOptions>(this IHostApplicationBuilder builder)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Configuration.GetOptions<TOptions>();
    }

    public static TOptions GetOptions<TOptions>(this IConfiguration configuration)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return (TOptions)Cache.GetOrAdd(typeof(TOptions), _ => CreateOptions<TOptions>(configuration));
    }

    private static TOptions CreateOptions<TOptions>(IConfiguration configuration)
        where TOptions : class
    {
        var type = typeof(TOptions);

        var instance = Activator.CreateInstance(
            type,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [configuration],
            culture: null);

        return instance as TOptions
            ?? throw new InvalidOperationException($"Could not create options type '{type.FullName}'.");
    }
}
