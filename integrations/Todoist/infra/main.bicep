param projectName string
param integrationName string
param sharedAppServicePlanName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param sharedServiceBusNamespaceName string
param todoistApiKeySecretName string
param todoistApiBaseUrl string
param setSubtaskLabelsSchedule string
param removeSubtaskLabelsSchedule string
param removeDueDateFromSubtasksSchedule string
param timeZone string

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
        value: '@Microsoft.KeyVault(VaultName=${sharedKeyVaultName};SecretName=${todoistApiKeySecretName})'
      }
      {
        name: 'SetSubtaskLabelsSchedule'
        value: setSubtaskLabelsSchedule
      }
      {
        name: 'RemoveSubtaskLabelsSchedule'
        value: removeSubtaskLabelsSchedule
      }
      {
        name: 'RemoveDueDateFromSubtasksSchedule'
        value: removeDueDateFromSubtasksSchedule
      }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    timeZone: timeZone
  }
}

module numberOfTasksServiceBusQueue '../../../shared-infra/modules/serviceBusQueue.bicep' = {
  name: 'numberOfTasksServiceBusQueueDeploy'
  params: {
    parentName: sharedServiceBusNamespaceName
    queueName: 'numberoftasks'
    projectName: projectName
    integrationName: integrationName
  }
}

output appName string = functionApp.outputs.appName
