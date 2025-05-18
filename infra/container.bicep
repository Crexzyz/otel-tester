param environmentName string
param location string
param uamiId string
param containerEnvId string
param registryName string
param appInsightsConnectionString string
param containerVersion string
param hostname string

resource containerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: '${environmentName}-container-${hostname}'
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
          image: '${registryName}.azurecr.io/${environmentName}:${containerVersion}'
          name: hostname
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
            {
              name: 'OTELTESTER_HOSTNAME'
              value: hostname
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
