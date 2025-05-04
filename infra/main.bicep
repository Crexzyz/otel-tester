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
var uamiName = '${environmentName}-uami'

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

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  location: location
  name: uamiName
}

resource readerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(uami.name, 'reader-role')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'acdd72a7-3385-48ef-bd42-f606fba81ae7' // Reader role ID
    )
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
    description: 'Reader role for the UAMI to read from the container registry'
  }
}

output environmentName string = environmentName
output location string = location
output uamiId string = uami.id
output containerEnvId string = containerEnv.id
output registryName string = containerRegistry.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
