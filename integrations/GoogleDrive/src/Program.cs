using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Integrations.GoogleDrive;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddAzureClients(b =>
{
    b.AddBlobServiceClient(builder.Configuration["StorageAccountConnectionString"]);
});

using var driveService = CreateDriveService();

builder.Services.AddSingleton(new DriveExportService(driveService));

builder.Build().Run();

return;

DriveService CreateDriveService()
{
    var json = builder.Configuration["GoogleDriveJsonCredentials"];

    var credential = CredentialFactory.FromJson<ServiceAccountCredential>(json)
        .ToGoogleCredential()
        .CreateScoped(DriveService.Scope.Drive);

    return new DriveService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential,
        ApplicationName = "Integration Platform"
    });
}
