param projectName string
param integrationName string
param sharedAppServicePlanName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param driveWorkJsonCredentialsSecretName string
param drivePersonalJsonCredentialsSecretName string
param googleApplicationName string
param concurrentDownloads int
param accountingDocumentationBackupCronSchedule string
param accountingDocumentationDriveFolderId string
param accountingDocumentationBackupContainerName string
param accountingDocumentationBackupFileNamePrefix string
param henrySavesBackupCronSchedule string
param henrySavesDriveFolderId string
param henrySavesBackupContainerName string
param henrySavesBackupFileNamePrefix string
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
        name: 'AccountingDocumentationBackupCronSchedule'
        value: accountingDocumentationBackupCronSchedule
      }
      {
        name: 'HenrySavesBackupCronSchedule'
        value: henrySavesBackupCronSchedule
      }
      {
        name: 'GoogleDrive__0__AccountType'
        value: 'Work'
      }
      {
        name: 'GoogleDrive__0__ApplicationName'
        value: googleApplicationName
      }
      {
        name: 'GoogleDrive__0__JsonCredentials'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${driveWorkJsonCredentialsSecretName})'
      }
      {
        name: 'GoogleDrive__0__ConcurrentDownloads'
        value: concurrentDownloads
      }
      {
        name: 'GoogleDrive__1__AccountType'
        value: 'Personal'
      }
      {
        name: 'GoogleDrive__1__ApplicationName'
        value: googleApplicationName
      }
      {
        name: 'GoogleDrive__1__JsonCredentials'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${drivePersonalJsonCredentialsSecretName})'
      }
      {
        name: 'GoogleDrive__1__ConcurrentDownloads'
        value: concurrentDownloads
      }
      {
        name: 'Backup__0__BackupType'
        value: 'AccountingDocumentation'
      }
      {
        name: 'Backup__0__AccountType'
        value: 'Work'
      }
      {
        name: 'Backup__0__DriveFolderId'
        value: accountingDocumentationDriveFolderId
      }
      {
        name: 'Backup__0__ContainerName'
        value: accountingDocumentationBackupContainerName
      }
      {
        name: 'Backup__0__FileNamePrefix'
        value: accountingDocumentationBackupFileNamePrefix
      }
      {
        name: 'Backup__1__BackupType'
        value: 'HenrySaves'
      }
      {
        name: 'Backup__1__AccountType'
        value: 'Personal'
      }
      {
        name: 'Backup__1__DriveFolderId'
        value: henrySavesDriveFolderId
      }
      {
        name: 'Backup__1__ContainerName'
        value: henrySavesBackupContainerName
      }
      {
        name: 'Backup__1__FileNamePrefix'
        value: henrySavesBackupFileNamePrefix
      }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    timeZone: timeZone
  }
}

output appName string = functionApp.outputs.appName
