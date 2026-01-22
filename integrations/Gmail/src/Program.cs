using Integrations.Gmail.Services;
using Integrations.Options;
using Integrations.Telemetry;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureApplicationInsightsTelemetry();
builder.ConfigureFunctionsWebApplication();

var options = builder.GetOptions<Options>();
builder.Services.Configure<SmtpOptions>(options.Smtp);

builder.Services.AddSingleton<EmailSenderService>();

builder.Build().Run();
