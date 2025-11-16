import { getResourceName } from '../../../shared-infra/functions.bicep'

param projectName string
param integrationName string
param sharedStorageAccountName string
param sharedKeyVaultName string
param todoistApiKeySecretName string
param todoistApiBaseUrl string
param timeZone string
param location string = resourceGroup().location

var name = '${projectName}-${integrationName}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' existing = {
  name: sharedStorageAccountName
}

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: sharedKeyVaultName
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

resource functionApp 'Microsoft.Web/sites@2024-11-01' = {
  name: getResourceName('func', name)
  kind: 'functionapp'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
        supportCredentials: true
      }
      appSettings: [
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_TIME_ZONE'
          value: timeZone
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'TodoistApiBaseUrl'
          value: todoistApiBaseUrl
        }
        {
          name: 'TodoistApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${todoistApiKeySecretName})'
        }
      ]
    }
    httpsOnly: true
  }
}

resource keyVaultAccessPolicies 'Microsoft.KeyVault/vaults/accessPolicies@2024-11-01' = {
  name: 'add'
  parent: keyVault
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: functionApp.identity.principalId
        permissions: {
          secrets: ['get']
        }
      }
    ]
  }
}

output appName string = functionApp.name
