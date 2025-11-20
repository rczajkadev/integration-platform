import { getStorageAccountName } from '../functions.bicep'

param projectName string
param integrationName string
param location string = resourceGroup().location

var name = '${projectName}-${integrationName}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' = {
  name: getStorageAccountName(name)
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

output name string = storageAccount.name
@secure()
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
