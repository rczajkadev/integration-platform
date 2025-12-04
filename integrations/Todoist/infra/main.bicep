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
param countTasksInProjectsSchedule string
param todoistNextActionsProjectId string
param todoistSomedayProjectId string
param todoistRecurringProjectId string
param timeZone string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: sharedServiceBusNamespaceName
}

resource serviceBusAuthorizationRules 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2024-01-01' existing = {
  name: 'RootManageSharedAccessKey'
  parent: serviceBusNamespace
}

module numberOfTasksServiceBusQueue '../../../shared-infra/modules/serviceBusQueue.bicep' = {
  name: 'numberOfTasksServiceBusQueueDeploy'
  params: {
    parentName: serviceBusNamespace.name
    queueName: 'numberoftasks'
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
        name: 'ServiceBusConnectionString'
        value: serviceBusAuthorizationRules.listKeys().primaryConnectionString
      }
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
      {
        name: 'CountTasksInProjectsSchedule'
        value: countTasksInProjectsSchedule
      }
      {
        name: 'NumberOfTasksServiceBusQueueName'
        value: numberOfTasksServiceBusQueue.outputs.name
      }
      {
        name: 'TodoistProjectIds__NextActions'
        value: todoistNextActionsProjectId
      }
      {
        name: 'TodoistProjectIds__Someday'
        value: todoistSomedayProjectId
      }
      {
        name: 'TodoistProjectIds__Recurring'
        value: todoistRecurringProjectId
      }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    timeZone: timeZone
  }
}

output appName string = functionApp.outputs.appName
