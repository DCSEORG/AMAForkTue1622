// Azure App Service for Expense Management System
// Using low-cost development SKU in UK South
// Following Azure best practices for App Service configuration

param location string = 'uksouth'
param appServicePlanName string = 'asp-expense-mgmt'
param webAppName string = 'app-expense-mgmt-${uniqueString(resourceGroup().id)}'
param managedIdentityId string
param managedIdentityClientId string

// App Service Plan - using Free tier for development
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'  // Free tier for development
    tier: 'Free'
    size: 'F1'
    family: 'F'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true  // Required for Linux
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true  // Best practice: enforce HTTPS
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'  // .NET 8 runtime
      alwaysOn: false  // Not available in Free tier
      ftpsState: 'Disabled'  // Best practice: disable FTP
      minTlsVersion: '1.2'  // Best practice: minimum TLS version
      http20Enabled: true
      appSettings: [
        {
          name: 'MANAGED_IDENTITY_CLIENT_ID'
          value: managedIdentityClientId
        }
        {
          name: 'SQL_SERVER'
          value: 'sql-expense-mgmt-xyz.database.windows.net'
        }
        {
          name: 'SQL_DATABASE'
          value: 'ExpenseManagementDB'
        }
      ]
    }
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppPrincipalId string = webApp.identity.principalId
