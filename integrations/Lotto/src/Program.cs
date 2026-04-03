using System;
using Integrations.Lotto;
using Integrations.Notifications;
using Integrations.Telemetry;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureApplicationInsightsTelemetry();
builder.ConfigureFunctionsWebApplication();

builder.Services.AddNotifications(
    builder.Configuration,
    optionsSectionName: "Notifications");

builder.Services.AddHttpClient<LottoClient>(client =>
{
    const string baseUrlPropertyName = "LottoBaseUrl";
    const string apiKeyPropertyName = "LottoApiKey";

    var baseUrlConfigValue = builder.Configuration[baseUrlPropertyName];
    var apiKeyConfigValue = builder.Configuration[apiKeyPropertyName];

    var baseUrl = !string.IsNullOrWhiteSpace(baseUrlConfigValue)
        ? baseUrlConfigValue
        : throw new InvalidOperationException($"'{baseUrlPropertyName}' missing in the configuration.");
    var apiKey = !string.IsNullOrWhiteSpace(apiKeyConfigValue)
        ? apiKeyConfigValue
        : throw new InvalidOperationException($"'{apiKeyPropertyName}' missing in the configuration.");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("secret", apiKey);
});

builder.Build().Run();
