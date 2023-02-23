
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

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

Write-Host "Deploying Azure Container Registry"
az deployment group create --resource-group $rg --template-file .\modules\containerRegistry.bicep --parameters acrName=$containerRegistryName

Write-Host "Pushing the source code to the container Registry and building the container there"

Push-Location $dir # temporarily change context to the current folder
az acr login --name $containerRegistryName
$dockerImageName = "$containerRegistryName.azurecr.io/aasonazurerepo:latest"
az acr build -t $dockerImageName --resource-group $rg --file ./../../src/aas-api-webapp-repository/Dockerfile ./../../

Pop-Location

Write-Host "Deploying Azure Services for functioning AAS Repository"

$myIpAddress = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
az deployment group create --resource-group $rg --template-file .\aasRepositoryAdt.bicep --parameters acrName=$containerRegistryName dockerImageName=$dockerImageName ipAddress=$myIpAddress
