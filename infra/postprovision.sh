#!/bin/bash

# Exit on error
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Load AZD environment variables
eval "$(azd env get-values)"

containerVersion="1.0.0"
deploymentName=$(az deployment group list \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --query "[0].name" -o tsv)

echo "⤵️  Postprovision variables: "
echo "AZURE_RESOURCE_GROUP: $AZURE_RESOURCE_GROUP"
echo "AZURE_REGION: $AZURE_REGION"
echo "AZURE_ENVIRONMENT: $AZURE_ENVIRONMENT"
echo "appInsightsConnectionString:${appInsightsConnectionString:+ (value hidden)}"
echo "containerEnvId: $containerEnvId"
echo "environmentName: $environmentName"
echo "location: $location"
echo "registryName: $registryName"
echo "uamiId: $uamiId"
echo "containerVersion: $containerVersion"
echo "deploymentName: $deploymentName"

# Build  and push the container image
echo "💿 Creating container image"
docker build \
    -t "$registryName.azurecr.io/$environmentName:$containerVersion" \
    -f "$SCRIPT_DIR/../api/Dockerfile" \
    "$SCRIPT_DIR/../api"

echo "⬆️  Pushing container image to ACR"
az acr login --name $registryName
docker push "$registryName.azurecr.io/$environmentName:$containerVersion"

for appName in "$@"; do
    echo "🌐  Creating container deployment for $appName"
    az deployment group create \
    --resource-group "$AZURE_RESOURCE_GROUP" \
    --name "container-$(date +%Y-%m-%d_%H-%M-%S)" \
    --template-file infra/container.bicep \
    --query "{status: properties.provisioningState, name: name}" \
    --output json \
    --parameters \
        environmentName="$environmentName" \
        location="$location" \
        uamiId="$uamiId" \
        containerEnvId="$containerEnvId" \
        registryName="$registryName" \
        appInsightsConnectionString="$appInsightsConnectionString" \
        containerVersion="$containerVersion" \
        hostname="$appName"
done
