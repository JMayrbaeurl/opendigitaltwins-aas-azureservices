# Open Digital Twins - Asset Administration Shell - Azure services

Sample [Industry 4.0 Asset Administration Shell REST API](https://www.plattform-i40.de/IP/Redaktion/EN/Downloads/Publikation/Details_of_the_Asset_Administration_Shell_Part2_V1.html) implementations for AAS Type 2 on [Azure Digital Twins](https://azure.microsoft.com/en-us/services/digital-twins/) using the [Open Digital Twins Asset Administration Shell ontology](https://github.com/JMayrbaeurl/opendigitaltwins-assetadminstrationshell)

The following Asset Administration Shell API implementations are are provided:

- **AASX File server**: See 'aas-api-webapp-aasxfile' folder in 'src'. Implements the 
[AASX File Server Interface](https://app.swaggerhub.com/apis/Plattform_i40/AssetAdministrationShell-REST-API/Final-Draft#/AASX%20File%20Server%20Interface/GetAllAASXPackageIds) 
using [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/). By default all AASX package files are stored as blobs 
in a container called `aasxfiles`. The name of the container to be used can be configured. If the container doesn't exist in the Blob storage account, 
it will be created automatically. For each package a folder with the file name without the file extension will be created in the container. 
Samples from [AASX Browser](https://admin-shell-io.com/5001/) were used for testing.
- **AAS Discovery server**: See 'aas-api-webapp-discovery' folder in 'src'. Partially implemented.
- **AAS Registry server**: See 'aas-api-webapp-registry' folder in 'src'. Partially implemented.
- **AAS Shell Repository server**: See 'aas-api-webapp-repository' folder in 'src'. Partially implemented.
- **AAS Full server**: See 'aas-api-webapp-full' folder in 'src'. Implementation of the entire interface collection as part of 
[Details of the Asset Administration Shell Part 2](https://www.plattform-i40.de/IP/Redaktion/EN/Downloads/Publikation/Details_of_the_Asset_Administration_Shell_Part2_V1.pdf)

## Security
All servers use [Default Azure credentials](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme) 
for authorization ([Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)) 
and must have according entries in the RBAC lists of the used Azure services (Azure Digital Twins and Azure Blob storage).

## Build and Run

Linux/OS X:

```
sh build.sh
```

Windows:

```
build.bat
```

## AASX File server
TBD.

The REST API operation `PostAASXPackage`of the AASX File Server Interface has some additional capabilities. 
If a valid download link (URL) to an AASX package file is provided in the parameter `fileName` and the parameter `file` is null, than the 
server will download the file directly to the storage. 

### Configuration
- Azure Blob storage configuration: Use `AASX_FILESERVICE_BLOBSTORAGEURL` in Application Settings to configure the url of the target
storage. E.g. `https://aasxstoragejm.blob.core.windows.net/`. By default a container titled `aasxfiles` will be used to store the aasx
packages. This can be changed by specifying another value in the Application settings for `AASX_FILESERVICE_CONTAINERNAME`. 
- Security: Beside the generic security setup (Azure AD App registration) the role `Storage Blob Data Contributor` has to be assigned 
to the AASX File server. E.g. by leveraging its Managed Identity of the App service.

## AAS Discovery server
TBD

## AAS Registry server
TBD

## AAS Shell Repository server
TBD

## AAS Full server
TBD
