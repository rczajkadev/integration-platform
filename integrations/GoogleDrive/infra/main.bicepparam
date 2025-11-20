using './main.bicep'

param projectName = 'int'
param integrationName = toLower('GoogleDrive')
param sharedAppServicePlanName = 'asp-int-shared'
param sharedStorageAccountName = 'stintsharedg7lv'
param sharedKeyVaultName = 'kv-int-shared-k4h7'
param googleDriveJsonCredentialsSecretName = 'GoogleDrive-GoogleDriveJsonCredentials'
param accountingDocumentationBackupCronSchedule = '0 0 2 1 * *'
param timeZone = 'Central European Standard Time'
