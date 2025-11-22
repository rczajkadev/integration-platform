param projectName string
param integrationName string
param sharedAppServicePlanName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param googleDriveJsonCredentialsSecretName string
param googleApplicationName string
param concurrentDownloads int
param accountingDocumentationDriveFolderId string
param accountingDocumentationBackupsContainerName string
param accountingDocumentationBackupsFileNamePrefix string
param accountingDocumentationBackupCronSchedule string
param timeZone string

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: sharedKeyVaultName
}

module storageAccount '../../../shared-infra/modules/storageAccount.bicep' = {
  name: 'storageAccountDeploy'
  params: {
    projectName: projectName
    integrationName: integrationName
  }
}

module functionApp '../../../shared-infra/modules/functionApp.bicep' = {
  name: 'functionAppDeploy'
  params: {
    projectName: projectName
    integrationName: integrationName
    customAppSettings: [
      {
        name: 'StorageAccountConnectionString'
        value: storageAccount.outputs.connectionString
      }
      {
        name: 'GoogleDriveJsonCredentials'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${googleDriveJsonCredentialsSecretName})'
      }
      {
        name: 'AccountingDocumentationBackupCronSchedule'
        value: accountingDocumentationBackupCronSchedule
      }
      {
        name: 'GoogleApplicationName'
        value: googleApplicationName
      }
      {
        name: 'ConcurrentDownloads'
        value: concurrentDownloads
      }
      {
        name: 'AccountingDocumentation__DriveFolderId'
        value: accountingDocumentationDriveFolderId
      }
      {
        name: 'AccountingDocumentation__BackupsContainerName'
        value: accountingDocumentationBackupsContainerName
      }
      {
        name: 'AccountingDocumentation__BackupsFileNamePrefix'
        value: accountingDocumentationBackupsFileNamePrefix
      }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    timeZone: timeZone
  }
}

output appName string = functionApp.outputs.appName
