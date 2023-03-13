
param (
	[string] $dcloc = "West Europe", 
	[string] $rg = "rg-test-aasrepo",

	[string] $containerRegistryName = "acr$(New-Guid)".Replace("-","")
)

$subscriptionResourceGroups=(az group list | ConvertFrom-Json)

if (-Not $subscriptionResourceGroups.name.Contains($rg)) {
	Write-Host "Deploying ressource group" $rg 
	az group create --name $rg --location $dcloc
}

Write-Host "Deploying Azure Services for functioning AAS Repository"

$dockerImageName="ghcr.io/mm-mse/aasonazurerepo"

$myIpAddress = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
az deployment group create --resource-group $rg --template-file .\aasRepositoryAdt.bicep --parameters dockerImageName=$dockerImageName ipAddress=$myIpAddress
