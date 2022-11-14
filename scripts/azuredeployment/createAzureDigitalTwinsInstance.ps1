# Create Azure Digital Twin instance for the Shell and Discovery service
#  uses the currently selected Azure subscription. Azure CLI version 2.14.0 or higher.

param (
	[string] $dcloc = "West Europe", 
	[string] $rg = "aas-sample-rg",
	[string] $dtName = "aasrepositorydigitaltwins"
)

Write-Host "`nStarting Deployment of Azure Digital Twins" $dtName "`n"

# 1. Check existenc of resource group and create it if it's not existing yet
$rsgExists = az group exists -n $rg
if ( $rsgExists -eq 'false') {
	Write-Host "Resource group " $rg " doesn't exist. Creating it now."
	az group create -l $dcloc -n $rg
}
else {
	Write-Host "Using existing Resource Group " $rg
}

# 2. Create Azure Digital Twin instance
$digitalTwinsExists = $(az dt list -g $rg --query "[?name=='$dtName']") | ConvertFrom-Json
if ($digitalTwinsExists.Length -eq 0 ) {
	az dt create --dt-name $dtName -g $rg -l $dcloc --assign-identity true
	Write-Host "Created Azure Digital Twins Instance " $dtName

	# 3. Create ADT Data Owner role for current user
	$currAcc = $(az account show) | ConvertFrom-Json
	az dt role-assignment create --dt-name $dtName -g $rg --assignee $currAcc.user.name --role "Azure Digital Twins Data Owner"
}
else {
	Write-Host "Digital Twins Instance " $dtName "already exists"
}