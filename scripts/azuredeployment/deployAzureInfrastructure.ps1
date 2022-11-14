param (
	[string] $dcloc = "West Europe", 
	[string] $rg = "aas-sample-rg",
	
	[string] $planName = "aasappserviceplan",
	[string] $planSku = "B1",
	[string] $appName = "aasapiservice",
	
	[string] $storageAccName = "aasxfileserverstorage",
	[string] $storageSku = "Standard_LRS",
	[string] $storageContainerName = "aasxfiles",
	
	[string] $dtName = "aasrepositorydigitaltwins",

	[string] $cacheName = "aasregistrycache",
	[string] $cacheSku = "Basic",
	[string] $cacheVmSize = "c0"
)

.\createWebApiAppForService.ps1 -dcloc $dcloc -rg $rg -planName $planName -planSku $planSku -appName $appName
.\createAzureStorageForAASXFileServer.ps1 -dcloc $dcloc -rg $rg -storageAccName $storageAccName -storageSku $storageSku
.\createAzureDigitalTwinsInstance.ps1 -dcloc $dcloc -rg $rg -dtName $dtname
.\createAzureRedisCacheDBForRegistryServer.ps1 -rg $rg -dcloc $dcloc -cacheName $cacheName -cacheSku $cacheSku -cacheVmSize $cacheVmSize -appName $appName