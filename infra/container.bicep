param environmentName string
param location string
param uamiId string
param containerEnvId string
param registryName string
param appInsightsConnectionString string

resource containerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: '${environmentName}-container'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uamiId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerEnvId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: '${registryName}.azurecr.io'
          identity: uamiId
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${registryName}.azurecr.io/${environmentName}:0.0.1'
          name: '${environmentName}-container1'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsightsConnectionString
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}
