// Main deployment template for Expense Management System
// Orchestrates deployment of all Azure resources

targetScope = 'resourceGroup'

param location string = 'uksouth'
param managedIdentityName string = 'mid-AppModAssist'
param appServicePlanName string = 'asp-expense-mgmt'
param webAppName string = 'app-expense-mgmt-${uniqueString(resourceGroup().id)}'

// Deploy Managed Identity
module managedIdentity './managed-identity.bicep' = {
  name: 'deploy-managed-identity'
  params: {
    location: location
    managedIdentityName: managedIdentityName
  }
}

// Deploy App Service
module appService './app-service.bicep' = {
  name: 'deploy-app-service'
  params: {
    location: location
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    managedIdentityId: managedIdentity.outputs.managedIdentityId
    managedIdentityClientId: managedIdentity.outputs.managedIdentityClientId
  }
}

output webAppName string = appService.outputs.webAppName
output webAppUrl string = appService.outputs.webAppUrl
output managedIdentityClientId string = managedIdentity.outputs.managedIdentityClientId
output managedIdentityPrincipalId string = managedIdentity.outputs.managedIdentityPrincipalId
