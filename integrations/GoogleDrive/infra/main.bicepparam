using './main.bicep'

param projectName = 'int'
param integrationName = toLower('GoogleDrive')
param sharedAppServicePlanName = 'asp-int-shared'
param sharedStorageAccountName = 'stintsharedg7lv'
param sharedKeyVaultName = 'kv-int-shared-k4h7'
param googleDriveJsonCredentialsSecretName = 'GoogleDrive-GoogleDriveJsonCredentials'
param googleApplicationName = 'Integration Platform'
param concurrentDownloads = 100
param accountingDocumentationDriveFolderId = '1k6XMCP0xjcy67bgEV6zAdxEUrdjm68Hv'
param accountingDocumentationBackupsContainerName = 'accounting-documentation-backups'
param accountingDocumentationBackupsFileNamePrefix = 'accounting-documentation-'
param accountingDocumentationBackupCronSchedule = '0 0 2 1 * *'
param timeZone = 'Central European Standard Time'
