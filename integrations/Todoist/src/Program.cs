using System.Net.Http.Headers;
using Integrations.Options;
using Integrations.Telemetry;
using Integrations.Todoist.Options;
using Integrations.Todoist.TodoistClient;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureApplicationInsightsTelemetry();
builder.ConfigureFunctionsWebApplication();

var options = builder.GetOptions<Options>();
builder.Services.Configure<TodoistProjectIdsOptions>(options.TodoistProjectIds);

builder.Services
    .AddRefitClient<ITodoistApi>(new RefitSettings
    {
        ContentSerializer = new NewtonsoftJsonContentSerializer()
    })
    .ConfigureHttpClient(client =>
    {
        var configuration = builder.Configuration;
        var baseUrl = configuration["TodoistApiBaseUrl"];
        var apiKey = configuration["TodoistApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Invalid Todoist API configuration");

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    });

builder.Build().Run();
