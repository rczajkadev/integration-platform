import { getResourceName } from '../functions.bicep'

param parentName string
param queueName string
param projectName string
param integrationName string

var name = '${projectName}-${integrationName}-${queueName}'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: parentName
}

resource sbqinttodoistnumberoftaskstmp 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  name: getResourceName('sbq', name)
  parent: serviceBusNamespace
  properties: {
    maxMessageSizeInKilobytes: 256
    lockDuration: 'PT1M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
    enableBatchedOperations: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    status: 'Active'
    enablePartitioning: false
    enableExpress: false
  }
}

output name string = sbqinttodoistnumberoftaskstmp.name
