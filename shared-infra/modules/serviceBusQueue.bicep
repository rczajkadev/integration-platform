import { getResourceName } from '../functions.bicep'

param parentName string
param queueName string
param projectName string
param integrationName string

var name = '${projectName}-${integrationName}-${queueName}'

resource sbqinttodoistnumberoftaskstmp 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  name: '${parentName}/${getResourceName('sbq', name)}'
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
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}
