param projectName string
param integrationName string
param sharedAppServicePlanName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param todoistApiKeySecretName string
param todoistApiBaseUrl string
param subtaskLabelsCheckSchedule string
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
        name: 'TodoistApiBaseUrl'
        value: todoistApiBaseUrl
      }
      {
        name: 'TodoistApiKey'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${todoistApiKeySecretName})'
      }
      {
        name: 'SubtaskLabelsCheckSchedule'
        value: subtaskLabelsCheckSchedule
      }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    timeZone: timeZone
  }
}

output appName string = functionApp.outputs.appName
