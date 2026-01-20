param projectName string
param integrationName string
param sharedAppServicePlanName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param sharedServiceBusNamespaceName string
param todoistApiKeySecretName string
param todoistApiBaseUrl string
param enforceSubtaskRulesSchedule string
param enforceRecurringTaskRulesSchedule string
param todoistRecurringProjectId string
param timeZone string

module functionApp '../../../infrastructure/modules/functionApp.bicep' = {
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
        value: '@Microsoft.KeyVault(VaultName=${sharedKeyVaultName};SecretName=${todoistApiKeySecretName})'
      }
      {
        name: 'EnforceSubtaskRulesSchedule'
        value: enforceSubtaskRulesSchedule
      }
      {
        name: 'EnforceRecurringTaskRulesSchedule'
        value: enforceRecurringTaskRulesSchedule
      }
      {
        name: 'TodoistProjectIds__Recurring'
        value: todoistRecurringProjectId
      }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    sharedServiceBusNamespaceName: sharedServiceBusNamespaceName
    timeZone: timeZone
  }
}

output appName string = functionApp.outputs.appName
