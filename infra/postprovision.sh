#!/bin/bash
set -e

# Load AZD environment variables
eval "$(azd env get-values)"

echo "🔍 Using resource group: $AZURE_RESOURCE_GROUP"

deploymentName=$(az deployment group list \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --query "[0].name" -o tsv)

echo "🔍 Using the latest deployment: $deploymentName"

# Fetch outputs from the initial deployment
outputs=$(az deployment group show \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$deploymentName" \
  --query "properties.outputs")

# Extract each output value using jq
environmentName=$(echo $outputs | jq -r '.environmentName.value')
location=$(echo $outputs | jq -r '.location.value')
uamiId=$(echo $outputs | jq -r '.uamiId.value')
containerEnvId=$(echo $outputs | jq -r '.containerEnvId.value')
registryName=$(echo $outputs | jq -r '.registryName.value')
appInsightsConn=$(echo $outputs | jq -r '.appInsightsConnectionString.value')

# Build  and push the container image
echo "Pushing container image to ACR"
az acr login --name $registryName
docker push "$registryName.azurecr.io/$environmentName:0.0.1"

# Deploy container app with those values
az deployment group create \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --name "$deploymentName-container" \
  --template-file infra/container.bicep \
  --parameters \
      environmentName="$environmentName" \
      location="$location" \
      uamiId="$uamiId" \
      containerEnvId="$containerEnvId" \
      registryName="$registryName" \
      appInsightsConnectionString="$appInsightsConn"
