using './main.bicep'

param projectName = 'int'
param integrationName = toLower('IntegrationTemplate')
param sharedAppServicePlanName = 'asp-int-shared'
param sharedStorageAccountName = 'stintsharedg7lv'
param sharedKeyVaultName = 'kv-int-shared-k4h7'
param sharedServiceBusNamespaceName = 'sbns-int-shared'
param sharedAppInsightsName = 'appi-int-shared'
param timeZone = 'Central European Standard Time'
param cronSchedule = '0 0 0 */7 * *'
