#!/bin/bash
# Deployment script for Expense Management System
# This script deploys all Azure infrastructure and the application

set -e  # Exit on error

echo "======================================"
echo "Expense Management System Deployment"
echo "======================================"
echo ""

# Variables
RESOURCE_GROUP="rg-expense-mgmt"
LOCATION="uksouth"
DEPLOYMENT_NAME="expense-mgmt-$(date +%Y%m%d-%H%M%S)"

# Check if Azure CLI is logged in
if ! az account show &> /dev/null; then
    echo "❌ Not logged in to Azure CLI. Please run 'az login' first."
    exit 1
fi

echo "✅ Azure CLI is logged in"
SUBSCRIPTION=$(az account show --query name -o tsv)
echo "📋 Using subscription: $SUBSCRIPTION"
echo ""

# Create resource group if it doesn't exist
echo "📦 Creating resource group: $RESOURCE_GROUP in $LOCATION..."
az group create --name $RESOURCE_GROUP --location $LOCATION --output none
echo "✅ Resource group ready"
echo ""

# Deploy infrastructure
echo "🚀 Deploying Azure infrastructure..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
  --name $DEPLOYMENT_NAME \
  --resource-group $RESOURCE_GROUP \
  --template-file ./infra/main.bicep \
  --query properties.outputs \
  --output json)

WEB_APP_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.webAppName.value')
WEB_APP_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.webAppUrl.value')
MI_CLIENT_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityClientId.value')
MI_PRINCIPAL_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityPrincipalId.value')

echo "✅ Infrastructure deployed successfully"
echo ""
echo "📋 Deployment Details:"
echo "   - Web App Name: $WEB_APP_NAME"
echo "   - Web App URL: $WEB_APP_URL"
echo "   - Managed Identity Client ID: $MI_CLIENT_ID"
echo "   - Managed Identity Principal ID: $MI_PRINCIPAL_ID"
echo ""

# Check if app.zip exists
if [ -f "./app.zip" ]; then
    echo "📦 Deploying application from app.zip..."
    az webapp deploy \
      --resource-group $RESOURCE_GROUP \
      --name $WEB_APP_NAME \
      --src-path ./app.zip \
      --type zip \
      --async false
    
    echo "✅ Application deployed successfully"
    echo ""
else
    echo "⚠️  app.zip not found. Skipping application deployment."
    echo "   Build the application first, then run:"
    echo "   az webapp deploy --resource-group $RESOURCE_GROUP --name $WEB_APP_NAME --src-path ./app.zip --type zip"
    echo ""
fi

echo "======================================"
echo "🎉 Deployment Complete!"
echo "======================================"
echo ""
echo "🌐 Access your application at:"
echo "   $WEB_APP_URL/Index"
echo ""
echo "⚠️  IMPORTANT: Navigate to /Index (not just the root URL)"
echo ""
echo "📝 Next Steps:"
echo "   1. Grant the managed identity access to your SQL database:"
echo "      - Principal ID: $MI_PRINCIPAL_ID"
echo "      - Run this in SQL Server: CREATE USER [mid-AppModAssist] FROM EXTERNAL PROVIDER;"
echo "      - Then: ALTER ROLE db_datareader ADD MEMBER [mid-AppModAssist];"
echo "      - And: ALTER ROLE db_datawriter ADD MEMBER [mid-AppModAssist];"
echo ""
