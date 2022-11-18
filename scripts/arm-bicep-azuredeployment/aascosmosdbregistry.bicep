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

@description('Web API name.')
@minLength(2)
param webAppName string = 'webApi-registry-${uniqueString(resourceGroup().id)}'

@description('The Runtime stack of current web app')
param linuxFxVersion string = 'DOCKER|aasapiimages.azurecr.io/aas-registry-server:latest'

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
  tags: {
    WorkloadName: 'AAS API Services'
    DataClassification: 'General'
    Criticality: 'Medium'
    ApplicationName: 'AAS Registry'
    Env: 'Test'
  }
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: locations
    databaseAccountOfferType: 'Standard'
  }
}
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' existing = {
  name: logs.outputs.logAnalyticsWorkspace
}

resource diagnosticLogs 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: account.name
  scope: account
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          days: 30
          enabled: true 
        }
      }
    ]
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  parent: account
  name: databaseName
  tags: {
    WorkloadName: 'AAS API Services'
    DataClassification: 'General'
    Criticality: 'Medium'
    ApplicationName: 'AAS Registry'
    Env: 'Test'
  }
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource containerShells 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  parent: database
  name: 'shells'
  tags: {
    WorkloadName: 'AAS API Services'
    DataClassification: 'General'
    Criticality: 'Medium'
    ApplicationName: 'AAS Registry'
    Env: 'Test'
  }
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
  tags: {
    WorkloadName: 'AAS API Services'
    DataClassification: 'General'
    Criticality: 'Medium'
    ApplicationName: 'AAS Registry'
    Env: 'Test'
  }
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

resource webApp 'Microsoft.Web/sites@2021-02-01' = {
  name: webAppName
  location: location
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.outputs.serverFarmId
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

module config 'modules/aasservicesconfig.bicep' = {
  name: 'aasservicesconfig'
  params: {
    location: location
  }
}

module logs 'modules/aasserviceslog.bicep' = {
    name: 'aasserviceslog'
    params: {
        location: location
    }
}

module apimgmt 'modules/aasapimgmt.bicep' = {
  name: 'aasservicesapimgmt'
  params: {
    location: location
  }
}

module appServicePlan 'modules/aasapiappserviceplan.bicep' = {
    name: 'aasservicesappsrvplan'
    params: {
    location: location
  }
}
