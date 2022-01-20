# Create Azure Web Api app for AAS Services
#  uses the currently selected Azure subscription. Azure CLI version 2.14.0 or higher.

param (
	[string] $dcloc = "West Europe", 
	[string] $rg = "aas-sample-rg",
	[string] $planName = "aasappserviceplan",
	[string] $planSku = "B1",
	[string] $appName = "aasapiservice"
)

# 1. Check existenc of resource group and create it if it's not existing yet
$rsgExists = az group exists -n $rg
if ( $rsgExists -eq 'false') {
	Write-Host "Resource group " $rg " doesn't exist. Creating it now."
	az group create -l $dcloc -n $rg
}

# 2. Create App service plan if it doesn't exist yet
$existingPlan=$(az appservice plan list -g $rg --query "[?name=='$planName']")
if ($existingPlan.length == 0) {
	az appservice plan create --name $planName --g $rg --location $dcloc --is-linux --sku $planSku
}

# 3. Create Web app
az webapp create -g $rg -p $planName -n $appName --runtime "DOTNETCORE|3.1" --assign-identity

# TODO Set 'kind' to "linux,api" with 'az resource update' to enable API features in portal

# 4. Create App registration for Web app
$clientId = $(az ad app create --display-name $appName --available-to-other-tenants false --app-roles @aasapiservicestdroles.json --query appId -o tsv)

# 5. Set Azure AD configurtion in Web app
az webapp config appsettings set --name $appName -g $rg --settings "AzureAd__ClientId=$clientId"