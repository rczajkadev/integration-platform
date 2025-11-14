using System.Net.Http.Headers;
using Integrations.Todoist;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddRefitClient<ITodoistApi>(new RefitSettings
    {
        ContentSerializer = new NewtonsoftJsonContentSerializer()
    })
    .ConfigureHttpClient(client =>
    {
        const string baseUrlPropertyName = "TodoistApiBaseUrl";
        const string apiKeyPropertyName = "TodoistApiKey";

        var baseUrl = builder.Configuration[baseUrlPropertyName] ?? throw new ArgumentNullException(baseUrlPropertyName);
        var apiKey = builder.Configuration[apiKeyPropertyName] ?? throw new ArgumentNullException(apiKeyPropertyName);

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    });

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
