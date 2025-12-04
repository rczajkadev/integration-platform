using System.Net.Http.Headers;
using Integrations.Todoist;
using Integrations.Todoist.Options;
using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddOptions<TodoistProjectIdsOptions>()
    .Bind(builder.Options.TodoistProjectIds)
    .Validate(
        o => !string.IsNullOrWhiteSpace(o.NextActions),
        "'TodoistProjectIds__NextActions' is required.")
    .Validate(
        o => !string.IsNullOrWhiteSpace(o.Someday),
        "'TodoistProjectIds__Someday' is required.")
    .Validate(
        o => !string.IsNullOrWhiteSpace(o.Recurring),
        "'TodoistProjectIds__Recurring' is required.")
    .ValidateOnStart();

builder.Services
    .AddRefitClient<ITodoistApi>(new RefitSettings
    {
        ContentSerializer = new NewtonsoftJsonContentSerializer()
    })
    .ConfigureHttpClient(client =>
    {
        var baseUrl = builder.Options.TodoistApiBaseUrl;
        var apiKey = builder.Options.TodoistApiKey;

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Invalid Todoist API configuration");

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    });

builder.Build().Run();
