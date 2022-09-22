param (
	[string] $rg = 'aasapi-work-rg',
	[string] $dbType = 'CosmosDB',
	[string] $clientID,
	[string] $appRegistrationName = 'aas-registryservice-test'
)

$clientID=$(.\createAASRegistryADAppRegIfNotExits.ps1 -appRegistrationName $appRegistrationName)

if ($dbType -eq 'Redis')
{
	az deployment group create --resource-group $rg --template-file .\aasredisregistry.bicep
}
else {
	Write-Output 'Deploying CosmosDB based AAS Registry'
	az deployment group create --resource-group $rg --template-file .\aascosmosdbregistry.bicep
}