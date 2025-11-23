import { getResourceName, getUniqueResourceName } from 'functions.bicep'

param projectName string
param integrationName string
param location string = resourceGroup().location

var name = '${projectName}-${integrationName}'

module storageAccount 'modules/storageAccount.bicep' = {
  name: 'storageAccountDeploy'
  params: {
    projectName: projectName
    integrationName: integrationName
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' = {
  name: getUniqueResourceName('kv', name)
  location: location
  properties: {
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enableRbacAuthorization: false
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: '51db1598-f913-46fa-970a-65ae40722481'
        permissions: {
          secrets: ['set', 'get', 'list', 'delete']
        }
      }
    ]
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: getResourceName('asp', name)
  kind: 'functionapp'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}
