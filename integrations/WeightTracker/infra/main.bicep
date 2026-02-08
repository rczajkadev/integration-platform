param projectName string
param integrationName string
param sharedAppServicePlanName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param sharedServiceBusNamespaceName string
param sharedAppInsightsName string
param notificationsEnabled bool
param notificationsBaseUrl string
param notificationsFunctionKeySecretName string
param weightTrackerApiBaseUrl string
param weightTrackerApiKeySecretName string
param checkStatusSchedule string
param timeZone string

module functionApp '../../../infrastructure/modules/functionApp.bicep' = {
  name: 'functionAppDeploy'
  params: {
    projectName: projectName
    integrationName: integrationName
    customAppSettings: [
      {
        name: 'WeightTrackerApiBaseUrl'
        value: weightTrackerApiBaseUrl
      }
      {
        name: 'WeightTrackerApiKey'
        value: '@Microsoft.KeyVault(VaultName=${sharedKeyVaultName};SecretName=${weightTrackerApiKeySecretName})'
      }
      {
        name: 'Notifications__Enabled'
        value: notificationsEnabled
      }
      {
        name: 'Notifications__BaseUrl'
        value: notificationsBaseUrl
      }
      {
        name: 'Notifications__FunctionKey'
        value: '@Microsoft.KeyVault(VaultName=${sharedKeyVaultName};SecretName=${notificationsFunctionKeySecretName})'
      }
      {
        name: 'CheckStatusSchedule'
        value: checkStatusSchedule
      }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    sharedServiceBusNamespaceName: sharedServiceBusNamespaceName
    sharedAppInsightsName: sharedAppInsightsName
    timeZone: timeZone
  }
}

output appName string = functionApp.outputs.appName
