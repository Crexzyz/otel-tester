@minLength(4)
@maxLength(32)
@description('Name of the the environment used to name other resources')
param environmentName string

@allowed(['westus', 'westus2', 'eastus', 'eastus2'])
@description('Location for all resources')
@metadata({
  azd: {
    type: 'location'
  }
})
param location string

var applicationType = 'web'
var appInsightsName = '${environmentName}-app-insights'
var logAnalyticsName = '${environmentName}-log-analytics'
var registryName = '${environmentName}acr'
var containerEnvName = '${environmentName}-acr-env'

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
}

// Application Insights instance
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: applicationType
    WorkspaceResourceId: logAnalytics.id
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: registryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

resource containerEnv 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: containerEnvName
  location: location
  properties: {}
}

// Export useful values
output connectionString string = appInsights.properties.ConnectionString
output instrumentationKey string = appInsights.properties.InstrumentationKey
output loginServer string = containerRegistry.properties.loginServer
