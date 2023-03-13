
@description('Location of all resources')
param location string = resourceGroup().location

@description('Azure Digital Twins Service name, max length 44 characters')
param adtName string = 'aasrepositoryadt-${uniqueString(resourceGroup().id)}'

@description('Azure Container App account name, max length 44 characters')

param dockerImageName string
@description('The IP Address that will be allowed to access the AAS repository API')
param ipAddress string


module adt 'modules/azureDigitalTwins.bicep'= {
  name: 'adtDeploy'
  scope: resourceGroup()
  params: {
    adtName: adtName
    location: location
  }
}

module containerApps 'modules/repositoryContainerAppAdt.bicep' = {
  name: 'containerApps'
  scope: resourceGroup()
  params: {
    location: location
    adtServiceUrl: 'https://${adt.outputs.hostName}'
    imageName: dockerImageName
    ipAddress: ipAddress
  } 
}

resource aksSubnetRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(adt.name,containerApps.name, resourceGroup().id)
  properties: {
    principalId: containerApps.outputs.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'bcd981a7-7f74-457b-83e1-cceb9e632ffe') //Azure Digital Twins Data Owner
  }
}
