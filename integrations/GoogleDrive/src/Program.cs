using Integrations.GoogleDrive.Backups;
using Integrations.GoogleDrive.Options;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<List<BackupOptions>>(builder.Configuration.GetSection(BackupOptions.SectionName));
builder.Services.Configure<List<GoogleDriveOptions>>(builder.Configuration.GetSection(GoogleDriveOptions.SectionName));

builder.Services.AddScoped<BackupOptionsResolver>();
builder.Services.AddScoped<BackupHandler>();

builder.Build().Run();
