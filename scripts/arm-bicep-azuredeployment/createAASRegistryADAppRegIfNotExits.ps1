param (
	[string] $appRegistrationName = 'aas-registryservice-test'
)

#objectid=$(az ad app show --id $clientid --query objectId --output tsv)

$appReg=$(az ad app list --filter "displayname eq '$appRegistrationName'" | ConvertFrom-Json)
$clientID=''

if ($appReg.length -eq 0) {
	Write-Host "No Azure AD App registration for '" $appRegistrationName "' found. Creating a new one now"

	$clientid=$(az ad app create --display-name $appRegistrationName --query appId --output tsv)
}
else {
	Write-Host "Found Azure AD App registration for '" $appRegistrationName "'."

	$clientid=$appReg[0].appId
}

Write-Host "Client ID = " $clientID

# TODO: Add the 'access_as_user' scope

# And give Azure CLI access to a API. See https://www.schaeflein.net/use-a-cli-to-get-an-access-token-for-your-aad-protected-web-api/
# by getting an access token like: az account get-access-token --resource api://f8ed554d-073a-49f8-971e-c6b3ca43e3d6

return $clientID