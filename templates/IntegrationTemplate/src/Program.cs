using Integrations.Telemetry;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureApplicationInsightsTelemetry();
builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
