
@description('Location of all resources')
param location string = resourceGroup().location

@description('Azure Digital Twins Service name, max length 44 characters')
param adtName string = 'aasrepositoryadt-${uniqueString(resourceGroup().id)}'

@description('Azure Container App account name, max length 44 characters')

param acrName string
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
    acrName: acrName
    adtServiceUrl: 'https://${adt.outputs.hostName}'
    imageName: dockerImageName
    ipAddress: ipAddress
  } 
}
