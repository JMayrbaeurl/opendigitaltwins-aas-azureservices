using AAS.AASX.Support;
using AdminShellNS;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.ADT
{
    public class ADTAASXPackageImporter : AASXImporter
    {
        private readonly DigitalTwinsClient dtClient;

        private readonly ILogger _logger;

        private static readonly string MODEL_CONCEPTDESCRIPTION = "dtmi:digitaltwins:aas:ConceptDescription;1";
        private static readonly string MODEL_DATASPECIEC61360 = "dtmi:digitaltwins:aas:DataSpecificationIEC61360;1";
        private static readonly string MODEL_REFERENCE = "dtmi:digitaltwins:aas:Reference;1";
        private static readonly string MODEL_KEY = "dtmi:digitaltwins:aas:Key;1";
        private static readonly string MODEL_ASSET = "dtmi:digitaltwins:aas:Asset;1";
        private static readonly string MODEL_SHELL = "dtmi:digitaltwins:aas:AssetAdministrationShell;1";
        private static readonly string MODEL_ASSETINFORMATION = "dtmi:digitaltwins:aas:AssetInformation;1";
        private static readonly string MODEL_SUBMODEL = "dtmi:digitaltwins:aas:Submodel;1";

        private readonly Dictionary<string, Dictionary<string, string>> DTIDMap = new Dictionary<string, Dictionary<string,string>>()
        {
            { $"{MODEL_CONCEPTDESCRIPTION}", new Dictionary<string, string>() { { "dtId", "ConceptDescription_" } } },
            { $"{MODEL_DATASPECIEC61360}", new Dictionary<string, string>() { { "dtId", "DataSpecIEC61360_" } } },
            { $"{MODEL_REFERENCE}", new Dictionary<string, string>() { { "dtId", "Reference_" } } },
            { $"{MODEL_KEY}", new Dictionary<string, string>() { { "dtId", "Key_" } } },
            { $"{MODEL_ASSET}", new Dictionary<string, string>() { { "dtId", "Asset_" } } },
            { $"{MODEL_SHELL}", new Dictionary<string, string>() { { "dtId", "Shell_" } } },
            { $"{MODEL_ASSETINFORMATION}", new Dictionary<string, string>() { { "dtId", "AssetInfo_" } } },
            { $"{MODEL_SUBMODEL}", new Dictionary<string, string>() { { "dtId", "Submodel_" } } }
        };

        public ADTAASXPackageImporter(DigitalTwinsClient adtClient, ILogger<ADTAASXPackageImporter> logger)
        {
            this.dtClient = adtClient;
            this._logger = logger;
        }

        public async Task<ImportResult> ImportFromPackageFile(string packageFilePath, bool ignConceptDescs)
        {
            ImportResult result = new ImportResult();

            using var package = new AdminShellPackageEnv(packageFilePath);

            if (package.AasEnv != null)
            {
                if (true)
                {
                    if (package.AasEnv.Assets != null && package.AasEnv.Assets.Count > 0)
                    {
                        _logger.LogInformation($"Now importing Assets.");

                        foreach (var asset in package.AasEnv.Assets)
                        {
                            try
                            {
                                await ImportAsset(asset, package, result);
                            }
                            catch (RequestFailedException ex)
                            {
                                _logger.LogError($"Exception create twin for Asset '{asset.idShort}': {ex.Message}", ex);
                            }
                        }

                        _logger.LogInformation($"Finished importing Assets.");
                    }

                    if (package.AasEnv.AdministrationShells != null && package.AasEnv.AdministrationShells.Count > 0)
                    {
                        _logger.LogInformation($"Now importing Administration shells.");

                        foreach (var shell in package.AasEnv.AdministrationShells)
                        {
                            try
                            {
                                await ImportShell(shell, package, result);
                            }
                            catch (RequestFailedException ex)
                            {
                                _logger.LogError($"Exception create twin for Administration shell '{shell.idShort}': {ex.Message}", ex);
                            }
                        }

                        _logger.LogInformation($"Finished importing Administration shells.");
                    }
                }

                if (!ignConceptDescs)
                {
                    if (package.AasEnv.ConceptDescriptions != null && package.AasEnv.ConceptDescriptions.Count > 0)
                    {
                        _logger.LogInformation($"Now importing Concept descriptions.");

                        foreach (var desc in package.AasEnv.ConceptDescriptions)
                        {
                            try
                            {
                                await ImportConceptDescription(desc, package, result);
                            }
                            catch (RequestFailedException ex)
                            {
                                _logger.LogError($"Exception create twin for Concept description '{desc.idShort}': {ex.Message}", ex);
                            }
                        }

                        _logger.LogInformation($"Finished importing Concept descriptions.");
                    }
                }
            }

            return result;
        }

        public async Task ImportShell(AdministrationShell shell, AdminShellPackageEnv package, ImportResult result)
        {
            _logger.LogInformation($"Now importing Administration shell '{shell.idShort}' into ADT instance");

            if (await ShellExists(shell))
            {
                _logger.LogInformation($"Skipping creation of twin for Administration shell '{shell.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Shell
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = MODEL_SHELL;
            twinData.Id = $"{DTIDMap[MODEL_SHELL]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(twinData, shell);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));

            // Create twins and relationships for embedded data specifications
            if (shell.hasDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, shell.hasDataSpecification, result);
            }

            // Create asset information
            if (shell.assetRef != null)
            {
                TwinRef<Asset> assetRef = result.AASAssets[shell.assetRef.First.idType+ shell.assetRef.First.value];

                var assetInfoTwinData = new BasicDigitalTwin();
                assetInfoTwinData.Metadata.ModelId = MODEL_ASSETINFORMATION;
                assetInfoTwinData.Id = $"{DTIDMap[MODEL_ASSETINFORMATION]["dtId"]}{Guid.NewGuid()}";
                BasicDigitalTwinComponent assetKind = new BasicDigitalTwinComponent();
                assetKind.Contents.Add("assetKind", assetRef.AASOject.kind.kind);
                assetInfoTwinData.Contents.Add("assetKind", assetKind);

                await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(assetInfoTwinData.Id, assetInfoTwinData);
                result.DTInstances.Add(new Tuple<string, string>(assetInfoTwinData.Id, assetInfoTwinData.Metadata.ModelId));

                // Create relationship from Shell to AssetInfo
                var relationship = new BasicRelationship
                {
                    TargetId = assetInfoTwinData.Id,
                    Name = "assetInformation"
                };

                string relId = $"{twinData.Id}-assetInformation->{assetInfoTwinData.Id}";
                await dtClient.CreateOrReplaceRelationshipAsync(twinData.Id, relId, relationship);

                // Create relationship from AssetInfo to Asset
                relationship = new BasicRelationship
                {
                    TargetId = assetRef.DtId,
                    Name = "globalAssetRef"
                };

                relId = $"{assetInfoTwinData.Id}-globalAssetRef->{assetRef.DtId}";
                await dtClient.CreateOrReplaceRelationshipAsync(assetInfoTwinData.Id, relId, relationship);
            }

            // Create Derived from reference
            if (shell.derivedFrom != null)
            {
                await AddReference(twinData, shell.derivedFrom, "derivedFrom", result);
            }

            // Create submodels from local references
            if (shell.submodelRefs != null && shell.submodelRefs.Count > 0)
            {
                foreach(var submodelRef in shell.submodelRefs)
                {
                    if (submodelRef.First.local && submodelRef.First.type == "Submodel")
                    {
                        Submodel submodel = FindSubmodelWithRef(submodelRef.First, shell, package);
                        if (submodel != null)
                        {
                            await ImportSubmodelFor(submodel, shell, package, twinData, result);
                        }
                    }
                }
            }
        }

        private async Task ImportSubmodelFor(Submodel submodel, AdministrationShell shell, AdminShellPackageEnv package,
            BasicDigitalTwin shellTwin, ImportResult result)
        {
            _logger.LogInformation($"Now importing Submodel '{submodel.idShort}' for shell '{shell.idShort}' into ADT instance");

            // Start by creating a twin for the Submodel
            BasicDigitalTwin subModelTwinData = new BasicDigitalTwin();
            subModelTwinData.Metadata.ModelId = MODEL_SUBMODEL;
            subModelTwinData.Id = $"{DTIDMap[MODEL_SUBMODEL]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(subModelTwinData, submodel);
            if (submodel.GetQualifiers() != null && submodel.GetQualifiers().Any())
                AddQualifiableAttributes(subModelTwinData, submodel.GetQualifiers());

            BasicDigitalTwinComponent assetKind = new BasicDigitalTwinComponent();
            assetKind.Contents.Add("kind", submodel.kind.kind);
            subModelTwinData.Contents.Add("kind", assetKind);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(subModelTwinData.Id, subModelTwinData);
            result.DTInstances.Add(new Tuple<string, string>(subModelTwinData.Id, subModelTwinData.Metadata.ModelId));

            // Create semantic Id
            if (submodel.semanticId != null)
            {
                await AddReference(subModelTwinData, submodel.semanticId, "semanticId", result);
            }

            // Create twins and relationships for embedded data specifications
            if (submodel.hasDataSpecification != null)
            {
                await AddHasDataSpecification(subModelTwinData, submodel.hasDataSpecification, result);
            }

            // TODO: Create Submodel elements
            if (submodel.submodelElements != null && submodel.submodelElements.Any())
            {
                foreach (var submodelElement in submodel.submodelElements)
                {
                    await ImportSubmodelElement(submodelElement, subModelTwinData, result);
                }
            }

            // Create relationship between Shell and Submodel
            var relationship = new BasicRelationship
            {
                TargetId = subModelTwinData.Id,
                Name = "submodel"
            };

            string relId = $"{shellTwin.Id}-submodel->{subModelTwinData.Id}";
            await dtClient.CreateOrReplaceRelationshipAsync(shellTwin.Id, relId, relationship);
        }

        private async Task ImportSubmodelElement(SubmodelElementWrapper submodelElement, BasicDigitalTwin subModelTwinData, ImportResult result)
        {
            
        }

        private Submodel FindSubmodelWithRef(Key submodelKey, AdministrationShell shell, AdminShellPackageEnv package)
        {
            Submodel result = package.AasEnv.Submodels.First<Submodel>(
                submodel => submodel.identification.id == submodelKey.value && submodel.identification.idType == submodelKey.idType );

            return result;
        }

        public async Task ImportAsset(Asset asset, AdminShellPackageEnv package, ImportResult result)
        {
            _logger.LogInformation($"Now importing Asset '{asset.idShort}' into ADT instance");

            if (await AssetExists(asset))
            {
                _logger.LogInformation($"Skipping creation of twin for Asset '{asset.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Asset
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = MODEL_ASSET;
            twinData.Id = $"{DTIDMap[MODEL_ASSET]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(twinData, asset);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));
            result.AASAssets.Add(asset.identification.idType + asset.identification.id, 
                new TwinRef<Asset>() { DtId = twinData.Id, AASOject = asset});

            // Create twins and relationships for embedded data specifications
            if (asset.hasDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, asset.hasDataSpecification, result);
            }
        }

        public async Task ImportConceptDescription(ConceptDescription conceptDescription, AdminShellPackageEnv package, ImportResult result)
        {
            _logger.LogInformation($"Now importing Concept description '{conceptDescription.idShort}' into ADT instance");

            if (await ConceptDescriptionExists(conceptDescription))
            {
                _logger.LogInformation($"Skipping creation of twin for Concept description '{conceptDescription.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Concept description
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = MODEL_CONCEPTDESCRIPTION;
            twinData.Id = $"{DTIDMap[MODEL_CONCEPTDESCRIPTION]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(twinData, conceptDescription,
                (conceptDescription.IEC61360Content != null && conceptDescription.IEC61360Content.shortName != null) ?
                conceptDescription.IEC61360Content.shortName.GetDefaultStr() : null);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));

            // Create twins and relationships for embedded data specifications
            if (conceptDescription.embeddedDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, conceptDescription.embeddedDataSpecification, result);
            }

            // IsCaseOf references
            if (conceptDescription.IsCaseOf != null)
            {
                await AddReferences(twinData, conceptDescription.IsCaseOf, "isCaseOf", result);
            }
        }

        private async Task AddHasDataSpecification(BasicDigitalTwin twinData, HasDataSpecification embeddedDataSpecification, ImportResult result)
        {
            if (embeddedDataSpecification.IEC61360Content != null)
            {
                DataSpecificationIEC61360 content = embeddedDataSpecification.IEC61360Content;

                // Create the DataSpecificationIEC61360 twin
                var dsTwinData = new BasicDigitalTwin();
                dsTwinData.Metadata.ModelId = MODEL_DATASPECIEC61360;
                dsTwinData.Id = $"{DTIDMap[MODEL_DATASPECIEC61360]["dtId"]}{Guid.NewGuid()}";

                if (content.preferredName != null)
                {
                    dsTwinData.Contents.Add("preferredName", LangStringSetIEC61360ToString(
                        content.preferredName));
                }
                if (content.shortName != null)
                {
                    dsTwinData.Contents.Add("shortName", LangStringSetIEC61360ToString(
                        content.shortName));
                }
                if (!string.IsNullOrEmpty(content.unit))
                    dsTwinData.Contents.Add("unit", content.unit);
                // TODO: unitId
                if (!string.IsNullOrEmpty(content.sourceOfDefinition))
                    dsTwinData.Contents.Add("sourceOfDefinition", content.sourceOfDefinition);
                if (!string.IsNullOrEmpty(content.symbol))
                    dsTwinData.Contents.Add("symbol", content.symbol);
                if (!string.IsNullOrEmpty(content.dataType))
                    dsTwinData.Contents.Add("dataType", content.dataType);
                if (content.definition != null)
                    dsTwinData.Contents.Add("definition", LangStringSetIEC61360ToString(content.definition));
                if (!string.IsNullOrEmpty(content.valueFormat))
                    dsTwinData.Contents.Add("valueFormat", content.valueFormat);
                // TODO: valueList
                // TODO: value
                // TODO: levelType

                await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(dsTwinData.Id, dsTwinData);
                result.DTInstances.Add(new Tuple<string, string>(dsTwinData.Id, dsTwinData.Metadata.ModelId));

                // Create relationship to 
                var relationship = new BasicRelationship
                {
                    TargetId = dsTwinData.Id,
                    Name = "dataSpecification"
                };

                string relId = $"{twinData.Id}-dataSpecification->{dsTwinData.Id}";
                await dtClient.CreateOrReplaceRelationshipAsync(twinData.Id, relId, relationship);
            }
        }

        private async Task AddReferences(BasicDigitalTwin twinData, List<Reference> references, string relationshipName, ImportResult result)
        {
            foreach(var reference in references)
            {
                await AddReference(twinData, reference, relationshipName, result);
            }
        }

        private async Task AddReference(BasicDigitalTwin twinData, Reference reference, string relationshipName, ImportResult result)
        {
            // Create Reference twin
            var refTwinData = new BasicDigitalTwin();
            refTwinData.Metadata.ModelId = MODEL_REFERENCE;
            refTwinData.Id = $"{DTIDMap[MODEL_REFERENCE]["dtId"]}{Guid.NewGuid()}";
            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(refTwinData.Id, refTwinData);
            result.DTInstances.Add(new Tuple<string, string>(refTwinData.Id, refTwinData.Metadata.ModelId));

            // Create relationship between source twin and Reference twin
            var relationship = new BasicRelationship
            {
                TargetId = refTwinData.Id,
                Name = relationshipName
            };

            string relId = $"{twinData.Id}-{relationshipName}->{refTwinData.Id}";
            await dtClient.CreateOrReplaceRelationshipAsync(twinData.Id, relId, relationship);

            await AddKey(refTwinData, reference, result);
        }

        private async Task AddKey(BasicDigitalTwin refTwinData, Reference reference, ImportResult result)
        {
            foreach (var key in reference.Keys)
            {
                string keyDtId = await KeyExists(key);
                if (keyDtId == null)
                {
                    // Create key
                    var keyTwinData = new BasicDigitalTwin();
                    keyTwinData.Metadata.ModelId = MODEL_KEY;
                    keyTwinData.Id = $"{DTIDMap[MODEL_KEY]["dtId"]}{Guid.NewGuid()}";
                    keyTwinData.Contents.Add("key", key.type);
                    keyTwinData.Contents.Add("value", key.value);
                    keyTwinData.Contents.Add("idType", URITOIRI(key.idType));
                    await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(keyTwinData.Id, keyTwinData);
                    result.DTInstances.Add(new Tuple<string, string>(keyTwinData.Id, keyTwinData.Metadata.ModelId));

                    keyDtId = keyTwinData.Id;
                }

                var keyRel = new BasicRelationship
                {
                    TargetId = keyDtId,
                    Name = "key"
                };
                
                string relId = $"{refTwinData.Id}-key->{keyDtId}";
                await dtClient.CreateOrReplaceRelationshipAsync(refTwinData.Id, relId, keyRel);
            }
        }

        private void AddReferableAttributes(BasicDigitalTwin twinData, Referable referable, string displayname = null)
        {
            if (!string.IsNullOrEmpty(referable.idShort))
                twinData.Contents.Add("idShort", referable.idShort);
            if (!string.IsNullOrEmpty(displayname))
                twinData.Contents.Add("displayName", displayname);
            if (!string.IsNullOrEmpty(referable.category))
                twinData.Contents.Add("category", referable.category);
            if (referable.description != null)
                twinData.Contents.Add("description", DescToString(referable.description));
        }

        private void AddIdentifiableAttributes(BasicDigitalTwin twinData, Identifiable identifiable, string displayname = null)
        {
            // Referable attributes
            AddReferableAttributes(twinData, identifiable, displayname);

            // Identifiable attributes
            BasicDigitalTwinComponent identifier = new BasicDigitalTwinComponent();
            if (identifiable.identification != null)
            {
                identifier.Contents.Add("id", identifiable.identification.id);
                identifier.Contents.Add("idType", identifiable.identification.idType);
            }
            twinData.Contents.Add("identification", identifier);

            BasicDigitalTwinComponent admin = new BasicDigitalTwinComponent();
            if (identifiable.administration != null &&
                (identifiable.administration.version != null || identifiable.administration.revision != null))
            {
                if (!string.IsNullOrEmpty(identifiable.administration.version))
                    admin.Contents.Add("version", identifiable.administration.version);
                if (!string.IsNullOrEmpty(identifiable.administration.revision))
                    admin.Contents.Add("revision", identifiable.administration.revision);
            }
            twinData.Contents.Add("administration", admin);
        }

        private void AddQualifiableAttributes(BasicDigitalTwin twinData, QualifierCollection qualifiers)
        {
            // TODO: To be implemented
            _logger.LogError("Adding qualifiers is not implemented yet.");
        }

        public static string DescToString(Description desc)
        {
            if (desc == null)
                return default(string);
            else
            {
                string result = "";
                foreach(var entry in desc.langString)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string LangStringSetIEC61360ToString(LangStringSetIEC61360 langStrs)
        {
            if (langStrs == null)
                return default(string);
            else
            {
                string result = "";
                foreach (var entry in langStrs)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string StripInvalidTwinIdCharacters(string dtIdProposal)
        {
            string result = dtIdProposal.Trim();

            result = result.Replace(" ", "");
            result = result.Replace("/", "");

            return result;
        }

        public static string URITOIRI(string idType)
        {
            if ("URI".Equals(idType))
                return "IRI";
            else
                return idType;
        }

        public async Task<bool> ConceptDescriptionExists(ConceptDescription conceptDescription)
        {
            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL(dt, '{MODEL_CONCEPTDESCRIPTION}') " +
                $"AND dt.identification.id = '{conceptDescription.identification.id}' " +
                $"AND dt.identification.idType = '{conceptDescription.identification.idType}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            bool result = await queryResult.GetAsyncEnumerator().MoveNextAsync();

            return result;
        }

        public async Task<bool> ShellExists(AdministrationShell shell)
        {
            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL(dt, '{MODEL_SHELL}') " +
                $"AND dt.identification.id = '{shell.identification.id}' " +
                $"AND dt.identification.idType = '{shell.identification.idType}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            bool result = await queryResult.GetAsyncEnumerator().MoveNextAsync();

            return result;
        }

        public async Task<bool> AssetExists(Asset asset)
        {
            bool result = false;

            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL(dt, '{MODEL_ASSET}') " +
                $"AND dt.identification.id = '{asset.identification.id}' " +
                $"AND dt.identification.idType = '{asset.identification.idType}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            result = await queryResult.GetAsyncEnumerator().MoveNextAsync();

            return result;
        }

        public async Task<string> KeyExists(Key key)
        {
            string result = null;

            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL('{MODEL_KEY}') " +
                $"AND key = '{key.type}' " + $"AND idType = '{URITOIRI(key.idType)}' " + $"AND value = '{key.value}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in queryResult)
            {
                result = twin.Id;
                break;
            }

            return result;
        }
    }
}
