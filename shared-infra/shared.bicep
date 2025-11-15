param projectName string
param integrationName string
param location string = resourceGroup().location

var resourceName = '${projectName}-${integrationName}'

var storageSuffixLength = 3
var uniqueStorageSuffix = take(uniqueString(subscription().id, location), storageSuffixLength)
var resourceNameWithoutHyphens = replace(resourceName, '-', '')
var storageAccountName = 'st${resourceNameWithoutHyphens}${uniqueStorageSuffix}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' = {
  name: 'kv-${resourceName}'
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
  }
}
