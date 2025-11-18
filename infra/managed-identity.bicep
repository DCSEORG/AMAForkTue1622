// User Assigned Managed Identity for App Service to connect to Azure SQL
// Following Azure best practices for managed identities

param location string = 'uksouth'
param managedIdentityName string = 'mid-AppModAssist'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

output managedIdentityId string = managedIdentity.id
output managedIdentityPrincipalId string = managedIdentity.properties.principalId
output managedIdentityClientId string = managedIdentity.properties.clientId
