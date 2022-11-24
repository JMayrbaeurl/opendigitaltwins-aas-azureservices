# Create Azure storage account for an AASX File Server
#  uses the currently selected Azure subscription. Azure CLI version 2.14.0 or higher.
#  Will create an ADLS storage account with hierarchical namespaces

param (
	[string] $dcloc = "West Europe", 
	[string] $rg = "aas-sample-rg",
	[string] $storageAccName = "aasxfileserverstorage",
	[string] $storageSku = "Standard_LRS",
	[string] $storageContainerName = "aasxfiles"
)

Write-Host "`nStarting Deployment of Storage " $storageAccName "`n"

# 1. Check existenc of resource group and create it if it's not existing yet
$rsgExists = az group exists -n $rg
if ( $rsgExists -eq 'false') {
	Write-Host "Resource group " $rg " doesn't exist. Creating it now."
	az group create -l $dcloc -n $rg
}
else {
	Write-Host "Using existing Resource Group " $rg
}

# 2. Create Azure storage account if not exists yet
$storageAccExists = $(az storage account check-name --name $storageAccName) | ConvertFrom-Json
if ( $storageAccExists.nameAvailable ) {
	Write-Host "Storage account " $storageAccName " doesn't exist. Creating it now."
	az storage account create -n $storageAccName -g $rg -l $dcloc --access-tier Hot --sku $storageSku --kind "StorageV2" `
	--allow-blob-public-access false --allow-shared-key-access false --enable-hierarchical-namespace true
}
else {
	Write-Host " Using existing Storage Account " $storageAccName 
}

# 3. Create container for AASX file packages
$contExists = $(az storage container exists --name aasxfiles --account-name $storageAccName --auth-mode login) | ConvertFrom-Json
if (-not($contExists.exists) ) {
	Write-Host "Container for AASX packages doesn't exist. Creating it now."
	az storage container create --name $storageContainerName --account-name $storageAccName --auth-mode login
}
else {
	Write-Host "Storage Container " $storageContainerName " already exists"
}

# 4. Assign roles to creating user
$currAcc = $(az account show) | ConvertFrom-Json
$storageAccId = $(az storage account show -n $storageAccName -g $rg --query id)
az role assignment create --role "Storage Blob Data Owner" --assignee $currAcc.user.name --scope $storageAccId

# 5. Set correct access
# $oldContAcc = $(az storage fs access show --account-name $storageAccName --file-system aasxfiles --path . --auth-mode login)
az storage fs access set --acl "user::rwx,group::r-x,other::r-x" --path . -f aasxfiles --account-name $storageAccName --auth-mode login
