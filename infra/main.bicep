@description('Name of the Application Insights resource')
param appInsightsName string = 'my-appinsights'

@description('Name of the Log Analytics workspace')
param logAnalyticsName string = 'my-law'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Application type for App Insights (e.g., web, other)')
param applicationType string = 'web'

// Log Analytics Workspace (2025-02-01 API version)
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  sku: {
    name: 'Free'
  }
  properties: {
    retentionInDays: 15
  }
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

// Export useful values
output connectionString string = appInsights.properties.ConnectionString
output instrumentationKey string = appInsights.properties.InstrumentationKey
