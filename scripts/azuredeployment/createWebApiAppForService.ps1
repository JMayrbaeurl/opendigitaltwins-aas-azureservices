# Create Azure Web Api app for AAS Services
#  uses the currently selected Azure subscription. Azure CLI version 2.14.0 or higher.

param (
	[string] $dcloc = "West Europe", 
	[string] $rg = "aas-sample-rg",
	[string] $planName = "aasappserviceplan",
	[string] $planSku = "B1",
	[string] $appName = "aasapiservice"
)

Write-Host "`nStarting Deployment of AASWebApi" $appName "`n"

# 1. Check existenc of resource group and create it if it's not existing yet
$rsgExists = az group exists -n $rg
if ( $rsgExists -eq 'false') {
	Write-Host "Resource group " $rg " doesn't exist. Creating it now."
	az group create -l $dcloc -n $rg
}
else {
	Write-Host "Using existing Ressource Group" $rg
}

# 2. Create App service plan if it doesn't exist yet
$existingPlans=$(az appservice plan list -g $rg --query "[?name=='$planName']") | ConvertFrom-Json
if ($existingPlans.Length -eq 0) {
	Write-Host "Creating App Service Plan " $planName
	az appservice plan create --name $planName -g $rg --location $dcloc --is-linux --sku $planSku
}
else {
	Write-Host "Using existing App Service Plan " $planName
}

# 3. Create Web app
$existingWebApps=$(az webapp list -g $rg --query "[?name=='$planName']") | ConvertFrom-Json
if ($existingWebApps.Length -eq 0) {
	Write-Host "Creating Web App" $appName
	az webapp create -g $rg -p $planName -n $appName --runtime "DOTNETCORE:3.1" --assign-identity
	# 4. Create App registration for Web app
	$clientId = $(az ad app create --display-name $appName --sign-in-audience 'AzureADMyOrg'  --app-roles '@aasapiservicestdroles.json' --query appId -o tsv)

	# 5. Set Azure AD configurtion in Web app
	az webapp config appsettings set --name $appName -g $rg --settings "AzureAd__ClientId=$clientId"
}
else {
	Write-Host "WebApp" $appName "already exists"
}
# TODO Set 'kind' to "linux,api" with 'az resource update' to enable API features in portal


