using './main.bicep'

param projectName = 'int'
param integrationName = toLower('Lotto')
param sharedAppServicePlanName = 'asp-int-shared'
param sharedStorageAccountName = 'stintsharedg7lv'
param sharedKeyVaultName = 'kv-int-shared-k4h7'
param sharedServiceBusNamespaceName = 'sbns-int-shared'
param sharedAppInsightsName = 'appi-int-shared'
param notificationsEnabled = true
param notificationsBaseUrl = 'https://func-int-gmail.azurewebsites.net/'
param notificationsFunctionKeySecretName = 'Common-SendEmailFunctionKey'
param lottoBaseUrl = 'https://developers.lotto.pl/api/'
param lottoApiKeySecretName = 'Lotto-LottoApiKey'
param timeZone = 'Central European Standard Time'
param checkLatestResultsSchedule = '0 0 23 * * 2,4,6'
