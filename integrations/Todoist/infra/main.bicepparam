using './main.bicep'

param projectName = 'int'
param integrationName = 'todoist'
param sharedStorageAccountName = 'stintsharedg7lv'
param sharedKeyVaultName = 'kv-int-shared-k4h7'
param todoistApiKeySecretName = 'Todoist-TodoistApiKey'
param todoistApiBaseUrl = 'https://api.todoist.com/api/v1'
param timeZone = 'Central European Standard Time'
