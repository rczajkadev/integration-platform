param projectName string
param integrationName string
param sharedAppServicePlanName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param sharedServiceBusNamespaceName string
param cronSchedule string
param timeZone string

module functionApp '../../../infrastructure/modules/functionApp.bicep' = {
  name: 'functionAppDeploy'
  params: {
    projectName: projectName
    integrationName: integrationName
    customAppSettings: [
      {
        name: 'CronSchedule'
        value: cronSchedule
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
