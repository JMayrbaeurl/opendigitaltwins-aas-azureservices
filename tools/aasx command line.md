# Asset Administration Shell - AASX Command line tool

A tool to work with Asset Administration Shell package files. E.g. importing contents of the files into an [Azure 
Digital Twin](https://docs.microsoft.com/en-us/azure/digital-twins/overview) instance using the 
[AAS DTDLv2 ontology](https://github.com/JMayrbaeurl/opendigitaltwins-assetadminstrationshell).

**Security**
The tool is using the following chain of Azure credentials:
- [EnvironmentCredential](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme-pre#environment-variables)
- [ManagedIdentityCredential](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme-pre#managed-identity-support)
- [AzureCliCredential](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme-pre#authenticating-via-development-tools)
- [InteractiveBrowserCredential](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme-pre#authenticating-users)

E.g. if you want to use Azure command line make sure, that you do a `az login` with Azure CLI before starting it the first time.

**Basic usage**
```
aasxcli subcommand [options]
```

Available subcommands are: `import` and `list-all`

| Command line syntax | Description |
| --- | --- |
| `aasxcli import` | Imports the contents of an AASX package into an Azure Digital Twin instance |
| `aasxcli list-all` | Lists the contents of an AASX package file |

**Global Parameters**

`--url -u`

Url of the Azure Digital Twins instance. E.g. `https://[Your ADT instance name].api.weu.digitaltwins.azure.net`

`--tenant -t`

Optional tenant Id of the Azure Digital Twins instance. Only needed for ADT instances in 'other' tenants.

`--file -f`

File path to the [AASX package file](https://github.com/admin-shell-io/aas-specs). 
Currently Version 1.0 and [Version 2.0](https://github.com/admin-shell-io/aasx-package-explorer/tree/master/src/AasxCsharpLibrary/Resources/schemaV201) 
of the AASX package file format are supported.

### aasxcli import

Imports the contents of the AASX package file into an Azure Digital Twin instance

```
aasxcli import --file --url [--ignoreConceptDescriptions] [--deleteShellsBeforeImport] [--automaticRelationships]
```
**Examples**

Import the '01 Festo' sample shells into ADT replacing existing nodes
```
aasxcli import -f ".\AASX Samples\01_Festo.aasx" --u "https://[Your ADT instance name].api.weu.digitaltwins.azure.net"
```

Import the '01 Festo' sample shells into ADT and delete the existing shell first
```
aasxcli import --deleteShellsBeforeImport -f ".\AASX Samples\01_Festo.aasx" --u "https://[Your ADT instance name].api.weu.digitaltwins.azure.net"
```
**Required Parameters**

None

**Optional Parameters**

`--ignoreConceptDescriptions`

Ignore the Concept Description entries in the AASX package file

`--deleteShellsBeforeImport`

Delete the existing shell twins in the Azure Digital Twins instance before importing the new one

`--automaticRelationships`

If true (default), Azure Digital Twins relationships will be automatically created for AAS Reference and ReferenceElements instances

### aasxcli list-all

Lists the contents of an AASX package file

```
aasxcli list-all --file --url
```
**Examples**

List all contents of the '01 Festo' sample
```
aasxcli list-all -f ".\AASX Samples\01_Festo.aasx" --u "https://[Your ADT instance name].api.weu.digitaltwins.azure.net"
```

```json
{
  "Shells": [
    {
      "IdType": "IRI",
      "Id": "smart.festo.com/demo/aas/1/1/454576463545648365874",
      "IdShort": "Festo_3S7PM0CP4BD"
    }
  ],
  "Submodels": [
    {
      "IdType": "IRI",
      "Id": "www.company.com/ids/sm/4343_5072_7091_3242",
      "IdShort": "Nameplate"
    },
    {
      "IdType": "IRI",
      "Id": "www.company.com/ids/sm/2543_5072_7091_2660",
      "IdShort": "Document"
    },
    {
      "IdType": "IRI",
      "Id": "www.company.com/ids/sm/6053_5072_7091_5102",
      "IdShort": "Service"
    },
    {
      "IdType": "IRI",
      "Id": "www.company.com/ids/sm/6563_5072_7091_4267",
      "IdShort": "Identification"
    },
    {
      "IdType": "IRI",
      "Id": "smart.festo.com/demo/sm/instance/1/1/13B7CCD9BF7A3F24",
      "IdShort": "DeviceDescriptionFiles"
    }
  ],
  "Assets": [
    {
      "IdType": "IRI",
      "Id": "HTTP://PK.FESTO.COM/3S7PM0CP4BD",
      "IdShort": "FPK_3s7plfdrs35"
    }
  ]
}
```

**Required Parameters**

--url is currently required but ignored

**Optional Parameters**

None
