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
param accountingDocumentationExportFolderId string
param accountingDocumentationBackupFolderId string
param accountingDocumentationBackupFileNamePrefix string
param henrySavesBackupCronSchedule string
param henrySavesExportFolderId string
param henrySavesBackupFolderId string
param henrySavesBackupFileNamePrefix string
param timeZone string

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: sharedKeyVaultName
}

module functionApp '../../../shared-infra/modules/functionApp.bicep' = {
  name: 'functionAppDeploy'
  params: {
    projectName: projectName
    integrationName: integrationName
    customAppSettings: [
      {
        name: 'KeyVaultUri'
        value: keyVault.properties.vaultUri
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
        name: 'GoogleDrive__0__KeyVaultSecretName'
        value: driveWorkJsonCredentialsSecretName
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
        name: 'GoogleDrive__1__KeyVaultSecretName'
        value: drivePersonalJsonCredentialsSecretName
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
        name: 'Backup__0__ExportFolderId'
        value: accountingDocumentationExportFolderId
      }
      {
        name: 'Backup__0__BackupFolderId'
        value: accountingDocumentationBackupFolderId
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
        name: 'Backup__1__ExportFolderId'
        value: henrySavesExportFolderId
      }
      {
        name: 'Backup__1__BackupFolderId'
        value: henrySavesBackupFolderId
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
