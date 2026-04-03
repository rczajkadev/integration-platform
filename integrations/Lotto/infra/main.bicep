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
param lottoBaseUrl string
param lottoApiKeySecretName string
param checkLatestResultsSchedule string
param timeZone string

module functionApp '../../../infrastructure/modules/functionApp.bicep' = {
  name: 'functionAppDeploy'
  params: {
    projectName: projectName
    integrationName: integrationName
    customAppSettings: [
      {
        name: 'LottoBaseUrl'
        value: lottoBaseUrl
      }
      {
        name: 'LottoApiKey'
        value: '@Microsoft.KeyVault(VaultName=${sharedKeyVaultName};SecretName=${lottoApiKeySecretName})'
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
        name: 'CheckLatestResultsSchedule'
        value: checkLatestResultsSchedule
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
