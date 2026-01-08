using Integrations.Gmail.Options;
using Integrations.Gmail.Services;
using Integrations.Options;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var options = builder.GetOptions<Options>();
builder.Services.Configure<SmtpOptions>(options.Smtp);

builder.Services.AddScoped<EmailSenderService>();

builder.Build().Run();
