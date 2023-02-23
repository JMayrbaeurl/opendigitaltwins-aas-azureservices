@description('Location of all resources')
param location string = resourceGroup().location

@description('Max length 44 characters')
param acrName string = 'aasonazurecr${uniqueString(resourceGroup().id)}'

resource aasonazurecr 'Microsoft.ContainerRegistry/registries@2022-12-01' = {
  sku: {
    name: 'Basic'
  }
  name: acrName
  location: location
  tags: {
  }
  properties: {
    adminUserEnabled: true
    encryption: {
      status: 'disabled'
    }
  }
}

output acrId string = aasonazurecr.id
