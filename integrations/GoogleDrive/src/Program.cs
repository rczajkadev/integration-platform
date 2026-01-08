using Integrations.GoogleDrive.Backups;
using Integrations.GoogleDrive.Options;
using Integrations.Options;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var options = builder.GetOptions<Options>();
builder.Services.Configure<List<BackupOptions>>(options.Backup);
builder.Services.Configure<List<GoogleDriveOptions>>(options.GoogleDrive);

builder.Services.AddAzureClients(azureBuilder =>
{
    const string keyVaultUriKey = "KeyVaultUri";
    var exception = new InvalidOperationException($"'{keyVaultUriKey}' not set");
    var keyVaultUri = builder.Configuration[keyVaultUriKey] ?? throw exception;
    azureBuilder.AddSecretClient(new Uri(keyVaultUri));
});

builder.Services.AddScoped<BackupOptionsResolver>();
builder.Services.AddScoped<BackupHandler>();

builder.Build().Run();
