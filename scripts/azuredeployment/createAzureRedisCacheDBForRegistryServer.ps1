# Create Azure Redis Cache DB for the AAS Registry service
#  uses the currently selected Azure subscription. Azure CLI version 2.14.0 or higher.
#  Will create an Azure Redis Cache DB (Basic C0). 
#  If parameter 'appName' contains a valid Web App name in the same resource group, it will 
#  automatically set the Web Apps config para 'AASREGISTRYCACHECONNSTRING' to the connection 
#  string of the created Redis Cache DB.

param (
	[string] $dcloc = "West Europe", 
	[string] $rg = "aas-sample-rg",
	[string] $cacheName = "aasregistrycache",
	[string] $cacheSku = "Basic",
	[string] $cacheVmSize = "c0",
	[string] $appName
)

# 1. Check existenc of resource group and create it if it's not existing yet
$rsgExists = az group exists -n $rg
if ( $rsgExists -eq 'false') {
	Write-Host "Resource group " $rg " doesn't exist. Creating it now."
	az group create -l $dcloc -n $rg
}

# 2. Create Redis Cache if it doesn't exist yet
$existingCaches=$(az redis list --query "[?name=='$cacheName']")
if ( $$existingCaches.length == 0) {
	Write-Host "Redis cache with name " $cacheName " doesn't exist. Creating it now"

	$redis=$(az redis create --location $dcloc --name $cacheName -g $rg `
	--sku $cacheSku --vm-size $cacheVmSize --redis-version 6 --query [hostName,sslPort] --output tsv)

# 3. Get connection string 
	$key=$(az redis list-keys --name $cacheName -g $rg --query primaryKey --output tsv)
	$connString=$redis[0] + ":" + $redis[1] + ",password=" + $key + ",ssl=True,abortConnect=False"
	Write-Host "Redis cache connection string: " $connString

# 4. Assign the connection string to an App Setting in the Web App
	if (-not $appName)
		az webapp config appsettings set --name $appName -g $rg --settings "AASREGISTRYCACHECONNSTRING=$connString"
}