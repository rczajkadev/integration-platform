namespace Integrations.GoogleDrive;

internal sealed class AccountingDocumentationOptions
{
    public const string SectionName = "AccountingDocumentation";

    public string DriveFolderId { get; init; } = null!;

    public string BackupsContainerName { get; init; } = null!;

    public string BackupsFileNamePrefix { get; init; } = null!;
}
