using System.Globalization;
using Integrations.GoogleDrive;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<AccountingDocumentationOptions>(
    builder.Configuration.GetSection(AccountingDocumentationOptions.SectionName));

builder.Services.AddAzureClients(b =>
{
    b.AddBlobServiceClient(builder.Configuration["StorageAccountConnectionString"]);
});

using var exportService = DriveExportService.Create(
    builder.Configuration["GoogleApplicationName"]!,
    builder.Configuration["GoogleDriveJsonCredentials"]!,
    int.Parse(builder.Configuration["ConcurrentDownloads"]!, CultureInfo.InvariantCulture));

builder.Services.AddSingleton(exportService);

builder.Build().Run();
