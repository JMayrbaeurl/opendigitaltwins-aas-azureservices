@description('Location of all resources')
param location string = resourceGroup().location

@description('Azure Cosmos DB account name, max length 44 characters')
param accountName string = 'aasregistrydb-${uniqueString(resourceGroup().id)}'

@description('The name for the database')
param databaseName string = 'aasregistrydb'

@description('The throughput policy for the container')
@allowed([
  'Manual'
  'Autoscale'
])
param throughputPolicy string = 'Autoscale'

@description('Throughput value when using Manual Throughput Policy for the container')
@minValue(400)
@maxValue(1000000)
param manualProvisionedThroughput int = 400

@description('Maximum throughput when using Autoscale Throughput Policy for the container')
@minValue(1000)
@maxValue(1000000)
param autoscaleMaxThroughput int = 1000

var locations = [
  {
    locationName: location
    failoverPriority: 0
    isZoneRedundant: false
  }
]
var throughput_Policy = {
  Manual: {
    throughput: manualProvisionedThroughput
  }
  Autoscale: {
    autoscaleSettings: {
      maxThroughput: autoscaleMaxThroughput
    }
  }
}

resource account 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: toLower(accountName)
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: locations
    databaseAccountOfferType: 'Standard'
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  parent: account
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource containerShells 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: database
  name: 'shells'
  properties: {
    resource: {
      id: 'shells'
      partitionKey: {
        paths: [
          '/shellDesc/identification'
        ]
        kind: 'Hash'
      }
    }
    options: throughput_Policy[throughputPolicy]
  }
}

resource containerSubmodels 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: database
  name: 'submodels'
  properties: {
    resource: {
      id: 'submodels'
      partitionKey: {
        paths: [
          '/shellDesc/identification'
        ]
        kind: 'Hash'
      }
    }
    options: throughput_Policy[throughputPolicy]
  }
}

module config 'modules/aasservicesconfig.bicep' = {
  name: 'aasservicesconfig'
  params: {
    location: location
  }
}