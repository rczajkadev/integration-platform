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
// Archived function: CountTasksInProjects
// param countTasksInProjectsSchedule string
// param todoistNextActionsProjectId string
// param todoistSomedayProjectId string
// param todoistRecurringProjectId string
param timeZone string

// Archived function: CountTasksInProjects
// module numberOfTasksServiceBusQueue '../../../infrastructure/modules/serviceBusQueue.bicep' = {
//   name: 'numberOfTasksServiceBusQueueDeploy'
//   params: {
//     parentName: sharedServiceBusNamespaceName
//     queueName: 'numberoftasks'
//     projectName: projectName
//     integrationName: integrationName
//   }
// }

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
      // Archived function: CountTasksInProjects
      // {
      //   name: 'CountTasksInProjectsSchedule'
      //   value: countTasksInProjectsSchedule
      // }
      // {
      //   name: 'NumberOfTasksServiceBusQueueName'
      //   value: numberOfTasksServiceBusQueue.outputs.name
      // }
      // {
      //   name: 'TodoistProjectIds__NextActions'
      //   value: todoistNextActionsProjectId
      // }
      // {
      //   name: 'TodoistProjectIds__Someday'
      //   value: todoistSomedayProjectId
      // }
      // {
      //   name: 'TodoistProjectIds__Recurring'
      //   value: todoistRecurringProjectId
      // }
    ]
    sharedAppServicePlanName: sharedAppServicePlanName
    sharedStorageAccountName: sharedStorageAccountName
    sharedKeyVaultName: sharedKeyVaultName
    sharedServiceBusNamespaceName: sharedServiceBusNamespaceName
    timeZone: timeZone
  }
}

output appName string = functionApp.outputs.appName
