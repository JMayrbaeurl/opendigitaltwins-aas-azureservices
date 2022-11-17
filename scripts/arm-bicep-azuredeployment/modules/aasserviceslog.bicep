@description('Specifies the name of the App Configuration store.')
param logWSName string = 'aaslogsws-${uniqueString(resourceGroup().id)}'

@description('Specifies the Azure location where the app configuration store should be created.')
param location string = resourceGroup().location

resource aaslogs 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
    name: logWSName
    location: location
    properties: {
        sku:{
	        name: 'PerGB2018'
        }
    }
    tags: {
        WorkloadName: 'AAS API Services'
        DataClassification: 'General'
        Criticality: 'Medium'
        ApplicationName: 'AAS Common'
        Env: 'Test'
    }
}

output logAnalyticsWorkspace string = logWSName