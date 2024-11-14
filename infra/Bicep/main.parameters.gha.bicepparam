// ----------------------------------------------------------------------------------------------------
// GitHub Workflow - Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using 'main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{generatedEnvCode}#'

param adInstance = '#{LOGIN_INSTANCEENDPOINT}#'
param adDomain = '#{LOGIN_DOMAIN}#'
param adTenantId = '#{LOGIN_TENANTID}#'
param adClientId = '#{LOGIN_CLIENTID}#'
param apiKey = '#{API_KEY}#'
param location = '#{RESOURCEGROUP_LOCATION}#'
