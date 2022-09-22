@description('Application Name')
@maxLength(30)
param applicationName string = 'aas-apiservices-${uniqueString(resourceGroup().id)}'

@description('Location for all resources.')
param location string = resourceGroup().location

@allowed([
  'F1'
  'D1'
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1'
  'P2'
  'P3'
  'P4'
])
@description('App Service Plan\'s pricing tier. Details at https://azure.microsoft.com/pricing/details/app-service/')
param appServicePlanTier string = 'B1'

@minValue(1)
@maxValue(3)
@description('App Service Plan\'s instance count')
param appServicePlanInstances int = 2

resource hostingPlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: applicationName
  location: location
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: appServicePlanTier
    capacity: appServicePlanInstances
  }
  tags: {
    WorkloadName: 'AAS API Services'
    DataClassification: 'General'
    Criticality: 'Medium'
    ApplicationName: 'AAS Common'
    Env: 'Test'
  }
}

output serverFarmId string = hostingPlan.id