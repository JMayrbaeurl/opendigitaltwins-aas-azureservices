@description('Location of all resources')
param location string = resourceGroup().location

@description('Specify the name of the Azure Redis Cache to create.')
param redisCacheName string = 'aasregistrydb-${uniqueString(resourceGroup().id)}'

resource registrydb 'Microsoft.Cache/redis@2021-06-01' = {
    location: location
    name: redisCacheName
    tags: {
        WorkloadName: 'AAS API Services'
        DataClassification: 'General'
        Criticality: 'Medium'
        ApplicationName: 'AAS Registry'
        Env: 'Test'
    }
    properties: {
    redisVersion: '6'
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    sku: {
      capacity: 0
      family: 'C'
      name: 'Basic'
    }
  }
}

module config 'modules/aasservicesconfig.bicep' = {
  name: 'aasservicesconfig'
  params: {
    location: location
  }
}