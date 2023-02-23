
@description('Location of all resources')
param location string = resourceGroup().location

@description('Azure Digital Twins Service name, max length 44 characters')
param adtName string = 'aasrepositoryadt-${uniqueString(resourceGroup().id)}'

@description('')
resource aasAdt 'Microsoft.DigitalTwins/digitalTwinsInstances@2022-10-31'= {
  location: location
  name: toLower(adtName)
  properties: {
    privateEndpointConnections: []
    publicNetworkAccess: 'Enabled'
  }
  identity: {
    type: 'SystemAssigned'
  }
}

output hostName string = aasAdt.properties.hostName
