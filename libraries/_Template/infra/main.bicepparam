using './main.bicep'

param projectName = 'int'
param integrationName = toLower('Template')
param sharedAppServicePlanName = 'asp-int-shared'
param sharedStorageAccountName = 'stintsharedg7lv'
param sharedKeyVaultName = 'kv-int-shared-k4h7'
param keyValultSecretName = 'Template-Secret'
param timeZone = 'Central European Standard Time'
param subtaskLabelsCheckSchedule = '0 0 2 * * *'
