
@description('Location of all resources')
param location string = resourceGroup().location

@description('Azure Container App Instance name, max length 32 characters')
param containerAppName string = 'repo${uniqueString(resourceGroup().id)}'

@description('Azure Container Environment name, max length 44 characters')
param containerEnvName string = 'containerenv-${uniqueString(resourceGroup().id)}'

param logAnalyticsWorkspaceName string = 'law-${uniqueString(resourceGroup().id)}'

@description('The full image name including the registry name')
param imageName string
param adtServiceUrl string
param ipAddress string

var ipAddressRange = '${ipAddress}/32'

resource law 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

@description('Generated from /subscriptions/17afe3cd-7b63-4a0a-b4f2-f66ce4a60383/resourceGroups/rg-project-AasOnAdt/providers/Microsoft.App/managedEnvironments/managedEnvironment-rgprojectAasOnA-866a')
resource managedEnvironmentrgprojectAasOnAa 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: containerEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: law.properties.customerId
        sharedKey: law.listKeys().primarySharedKey
      }
    }
    zoneRedundant: false
    customDomainConfiguration: {
    }
  }
  sku: {
    name: 'Consumption'
  }
}

resource aaswebapprepository 'Microsoft.App/containerApps@2022-10-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: managedEnvironmentrgprojectAasOnAa.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        exposedPort: 0
        transport: 'Auto'
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
        allowInsecure: false
        ipSecurityRestrictions: [
          {
            name: 'My IP'
            ipAddressRange: ipAddressRange
            action: 'Allow'
          }
        ]
      }
    }
    template: {
      revisionSuffix: ''
      containers: [
        {
          image: imageName
          name: 'aas-webapp-repository'
          env: [
            {
              name: 'ADT_SERVICE_URL'
              value: adtServiceUrl
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          probes: []
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

output principalId string = aaswebapprepository.identity.principalId
