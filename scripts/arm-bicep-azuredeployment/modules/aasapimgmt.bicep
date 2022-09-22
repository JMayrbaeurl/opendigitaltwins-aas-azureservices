@description('The name of the API Management service instance')
param apiManagementServiceName string = 'aasapiservice${uniqueString(resourceGroup().id)}'

@description('The email address of the owner of the service')
@minLength(1)
param publisherEmail string = 'jurgenma@microsoft.com'

@description('The name of the owner of the service')
@minLength(1)
param publisherName string = 'Juergen Mayrbaeurl'

@description('The pricing tier of this API Management service')
@allowed([
  'Developer'
  'Standard'
  'Premium'
])
param sku string = 'Standard'

@description('The instance size of this API Management service.')
@allowed([
  1
  2
])
param skuCount int = 1

@description('Location for all resources.')
param location string = resourceGroup().location

resource apiManagementService 'Microsoft.ApiManagement/service@2021-08-01' = {
  name: apiManagementServiceName
  location: location
  sku: {
    name: sku
    capacity: skuCount
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
  }
  tags: {
    WorkloadName: 'AAS API Services'
    DataClassification: 'General'
    Criticality: 'Medium'
    ApplicationName: 'AAS Common'
    Env: 'Test'
  }
}