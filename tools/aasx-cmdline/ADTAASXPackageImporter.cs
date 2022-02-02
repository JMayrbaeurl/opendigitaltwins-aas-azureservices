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
    public class ADTAASXPackageImporter : AbstractADTCommand, AASXImporter
    {
        private readonly ILogger _logger;

        public ADTAASXPackageImporter(DigitalTwinsClient adtClient, ILogger<ADTAASXPackageImporter> logger) : base(adtClient)
        {
            this._logger = logger;
        }

        public async Task<ImportResult> ImportFromPackageFile(string packageFilePath, ImportContext processInfo)
        {
            using var package = new AdminShellPackageEnv(packageFilePath);

            if (package.AasEnv != null)
            {
                if (!processInfo.Configuration.IgnoreShells)
                {
                    if (package.AasEnv.Assets != null && package.AasEnv.Assets.Count > 0)
                    {
                        _logger.LogInformation($"Now importing Assets.");

                        foreach (var asset in package.AasEnv.Assets)
                        {
                            try
                            {
                                await ImportAsset(asset, package, processInfo);
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
                                if (processInfo.Configuration.DeleteShellBeforeImport)
                                    await DeleteShell(shell, package);

                                await ImportShell(shell, package, processInfo);
                            }
                            catch (RequestFailedException ex)
                            {
                                _logger.LogError($"Exception create twin for Administration shell '{shell.idShort}': {ex.Message}", ex);
                            }
                        }

                        _logger.LogInformation($"Finished importing Administration shells.");
                    }
                }

                if (!processInfo.Configuration.IgnoreConceptDescriptions)
                {
                    if (package.AasEnv.ConceptDescriptions != null && package.AasEnv.ConceptDescriptions.Count > 0)
                    {
                        _logger.LogInformation($"Now importing Concept descriptions.");

                        foreach (var desc in package.AasEnv.ConceptDescriptions)
                        {
                            try
                            {
                                await ImportConceptDescription(desc, package, processInfo);
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

            return processInfo.Result;
        }

        public async Task ImportShell(AdministrationShell shell, AdminShellPackageEnv package, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing Administration shell '{shell.idShort}' into ADT instance");

            if (await ShellExists(shell))
            {
                _logger.LogInformation($"Skipping creation of twin for Administration shell '{shell.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Shell
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = ADTAASOntology.MODEL_SHELL;
            twinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_SHELL]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(twinData, shell);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            processInfo.Result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));

            // Create twins and relationships for embedded data specifications
            if (shell.hasDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, shell.hasDataSpecification, processInfo);
            }

            // Create asset information
            if (shell.assetRef != null)
            {
                TwinRef<Asset> assetRef = processInfo.Result.AASAssets[shell.assetRef.First.idType+ shell.assetRef.First.value];

                var assetInfoTwinData = new BasicDigitalTwin();
                assetInfoTwinData.Metadata.ModelId = ADTAASOntology.MODEL_ASSETINFORMATION;
                assetInfoTwinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_ASSETINFORMATION]["dtId"]}{Guid.NewGuid()}";
                BasicDigitalTwinComponent assetKind = new BasicDigitalTwinComponent();
                assetKind.Contents.Add("assetKind", assetRef.AASOject.kind.kind);
                assetInfoTwinData.Contents.Add("assetKind", assetKind);

                await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(assetInfoTwinData.Id, assetInfoTwinData);
                processInfo.Result.DTInstances.Add(new Tuple<string, string>(assetInfoTwinData.Id, assetInfoTwinData.Metadata.ModelId));

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
                await AddReference(twinData, shell.derivedFrom, "derivedFrom", processInfo);
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
                            await ImportSubmodelFor(submodel, shell, package, twinData, processInfo);
                        }
                    }
                }
            }
        }

        private async Task ImportSubmodelFor(Submodel submodel, AdministrationShell shell, AdminShellPackageEnv package,
            BasicDigitalTwin shellTwin, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing Submodel '{submodel.idShort}' for shell '{shell.idShort}' into ADT instance");

            // Start by creating a twin for the Submodel
            BasicDigitalTwin subModelTwinData = new BasicDigitalTwin();
            subModelTwinData.Metadata.ModelId = ADTAASOntology.MODEL_SUBMODEL;
            subModelTwinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_SUBMODEL]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(subModelTwinData, submodel);
            if (submodel.GetQualifiers() != null && submodel.GetQualifiers().Any())
                AddQualifiableAttributes(subModelTwinData, submodel.GetQualifiers());

            BasicDigitalTwinComponent assetKind = new BasicDigitalTwinComponent();
            assetKind.Contents.Add("kind", submodel.kind.kind);
            subModelTwinData.Contents.Add("kind", assetKind);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(subModelTwinData.Id, subModelTwinData);
            processInfo.Result.DTInstances.Add(new Tuple<string, string>(subModelTwinData.Id, subModelTwinData.Metadata.ModelId));

            // Create semantic Id
            if (submodel.semanticId != null)
            {
                await AddReference(subModelTwinData, submodel.semanticId, "semanticId", processInfo);
            }

            // Create twins and relationships for embedded data specifications
            if (submodel.hasDataSpecification != null)
            {
                await AddHasDataSpecification(subModelTwinData, submodel.hasDataSpecification, processInfo);
            }

            // TODO: Create Submodel elements
            if (submodel.submodelElements != null && submodel.submodelElements.Any())
            {
                foreach (var submodelElement in submodel.submodelElements)
                {
                    string submodelElementDtId = await ImportSubmodelElement(submodelElement, subModelTwinData, processInfo);
                    if (submodelElementDtId != null)
                    {
                        // Create relationship between Submodel and SubmodelElement
                        var rs = new BasicRelationship
                        {
                            TargetId = submodelElementDtId,
                            Name = "submodelElement"
                        };

                        string relationId = $"{subModelTwinData.Id}-submodelElement->{submodelElementDtId}";
                        await dtClient.CreateOrReplaceRelationshipAsync(subModelTwinData.Id, relationId, rs);
                    }
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

        private async Task<string> ImportSubmodelElement(SubmodelElementWrapper submodelElementWrapper, BasicDigitalTwin subModelTwinData, ImportContext processInfo)
        {
            if (submodelElementWrapper.submodelElement.GetType() == typeof(SubmodelElementCollection))
            {
                return await ImportSubmodelElementCollection(submodelElementWrapper.GetAs<SubmodelElementCollection>(), submodelElementWrapper, subModelTwinData, processInfo);
            } 
            else if (submodelElementWrapper.submodelElement.GetType() == typeof (Property))
            {
                return await ImportProperty(submodelElementWrapper.GetAs<Property>(), submodelElementWrapper, subModelTwinData, processInfo);
            }

            _logger.LogError($"ImportSubmodelElement called for unsupported SubmodelElement '{submodelElementWrapper.submodelElement.GetType()}'");

            return null;
        }

        private async Task<string> ImportSubmodelElementCollection(SubmodelElementCollection submodelElementCollection, SubmodelElementWrapper submodelElementWrapper, BasicDigitalTwin subModelTwinData, ImportContext processInfo)
        {
            _logger.LogError("ImportSubmodelElementCollection not implemented yet");

            return null;
        }

        private async Task<string> ImportProperty(Property property, SubmodelElementWrapper submodelElementWrapper, BasicDigitalTwin subModelTwinData, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing property '{property.idShort}'");

            // Start by creating a twin for the Asset
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = ADTAASOntology.MODEL_PROPERTY;
            twinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_PROPERTY]["dtId"]}{Guid.NewGuid()}";

            AddDataElementAttributes(twinData, property);

            if (property.valueType != null)
                twinData.Contents.Add("valueType", property.valueType);
            if (property.value != null)
                twinData.Contents.Add("value", property.value);
            if (property.valueId != null)
                await AddReference(twinData, property.valueId, "valueId", processInfo);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            processInfo.Result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));

            await AddDataElementRelationships(twinData, property, processInfo);

            return twinData.Id;
        }

        private void AddDataElementAttributes(BasicDigitalTwin twinData, DataElement dataElement)
        {
            AddSubmodelElementAttributes(twinData, dataElement);
        }

        private async Task AddDataElementRelationships(BasicDigitalTwin twinData, DataElement dataElement, ImportContext processInfo)
        {
            await AddSubmodelElementRelationships(twinData, dataElement, processInfo);
        }

        private void AddSubmodelElementAttributes(BasicDigitalTwin twinData, SubmodelElement submodelElement)
        {
            AddReferableAttributes(twinData, submodelElement);
            if (submodelElement.GetQualifiers() != null && submodelElement.GetQualifiers().Any())
                AddQualifiableAttributes(twinData, submodelElement.GetQualifiers());

            BasicDigitalTwinComponent kind = new BasicDigitalTwinComponent();
            kind.Contents.Add("kind", submodelElement.kind.kind);
            twinData.Contents.Add("kind", kind);
        }

        private async Task AddSubmodelElementRelationships(BasicDigitalTwin twinData, SubmodelElement submodelElement, ImportContext processInfo)
        {
            // Create semantic Id
            if (submodelElement.semanticId != null)
            {
                await AddReference(twinData, submodelElement.semanticId, "semanticId", processInfo);
            }

            if (submodelElement.hasDataSpecification != null)
                await AddHasDataSpecification(twinData, submodelElement.hasDataSpecification, processInfo);
        }

        private Submodel FindSubmodelWithRef(Key submodelKey, AdministrationShell shell, AdminShellPackageEnv package)
        {
            Submodel result = package.AasEnv.Submodels.First<Submodel>(
                submodel => submodel.identification.id == submodelKey.value && submodel.identification.idType == submodelKey.idType );

            return result;
        }

        public async Task ImportAsset(Asset asset, AdminShellPackageEnv package, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing Asset '{asset.idShort}' into ADT instance");

            if (await AssetExists(asset))
            {
                _logger.LogInformation($"Skipping creation of twin for Asset '{asset.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Asset
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = ADTAASOntology.MODEL_ASSET;
            twinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_ASSET]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(twinData, asset);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            processInfo.Result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));
            processInfo.Result.AASAssets.Add(asset.identification.idType + asset.identification.id, 
                new TwinRef<Asset>() { DtId = twinData.Id, AASOject = asset});

            // Create twins and relationships for embedded data specifications
            if (asset.hasDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, asset.hasDataSpecification, processInfo);
            }
        }

        public async Task ImportConceptDescription(ConceptDescription conceptDescription, AdminShellPackageEnv package, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing Concept description '{conceptDescription.idShort}' into ADT instance");

            if (await ConceptDescriptionExists(conceptDescription))
            {
                _logger.LogInformation($"Skipping creation of twin for Concept description '{conceptDescription.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Concept description
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = ADTAASOntology.MODEL_CONCEPTDESCRIPTION;
            twinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_CONCEPTDESCRIPTION]["dtId"]}{Guid.NewGuid()}";

            AddIdentifiableAttributes(twinData, conceptDescription,
                (conceptDescription.IEC61360Content != null && conceptDescription.IEC61360Content.shortName != null) ?
                conceptDescription.IEC61360Content.shortName.GetDefaultStr() : null);

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            processInfo.Result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));

            // Create twins and relationships for embedded data specifications
            if (conceptDescription.embeddedDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, conceptDescription.embeddedDataSpecification, processInfo);
            }

            // IsCaseOf references
            if (conceptDescription.IsCaseOf != null)
            {
                await AddReferences(twinData, conceptDescription.IsCaseOf, "isCaseOf", processInfo);
            }
        }

        private async Task AddHasDataSpecification(BasicDigitalTwin twinData, HasDataSpecification embeddedDataSpecification, ImportContext processInfo)
        {
            if (embeddedDataSpecification.IEC61360Content != null)
            {
                DataSpecificationIEC61360 content = embeddedDataSpecification.IEC61360Content;

                // Create the DataSpecificationIEC61360 twin
                var dsTwinData = new BasicDigitalTwin();
                dsTwinData.Metadata.ModelId = ADTAASOntology.MODEL_DATASPECIEC61360;
                dsTwinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_DATASPECIEC61360]["dtId"]}{Guid.NewGuid()}";

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
                processInfo.Result.DTInstances.Add(new Tuple<string, string>(dsTwinData.Id, dsTwinData.Metadata.ModelId));

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

        private async Task AddReferences(BasicDigitalTwin twinData, List<Reference> references, string relationshipName, ImportContext processInfo)
        {
            foreach(var reference in references)
            {
                await AddReference(twinData, reference, relationshipName, processInfo);
            }
        }

        private async Task AddReference(BasicDigitalTwin twinData, Reference reference, string relationshipName, ImportContext processInfo)
        {
            // Create Reference twin
            var refTwinData = new BasicDigitalTwin();
            refTwinData.Metadata.ModelId = ADTAASOntology.MODEL_REFERENCE;
            refTwinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_REFERENCE]["dtId"]}{Guid.NewGuid()}";
            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(refTwinData.Id, refTwinData);
            processInfo.Result.DTInstances.Add(new Tuple<string, string>(refTwinData.Id, refTwinData.Metadata.ModelId));

            // Create relationship between source twin and Reference twin
            var relationship = new BasicRelationship
            {
                TargetId = refTwinData.Id,
                Name = relationshipName
            };

            string relId = $"{twinData.Id}-{relationshipName}->{refTwinData.Id}";
            await dtClient.CreateOrReplaceRelationshipAsync(twinData.Id, relId, relationship);

            await AddKey(refTwinData, reference, processInfo);
        }

        private async Task AddKey(BasicDigitalTwin refTwinData, Reference reference, ImportContext processInfo)
        {
            foreach (var key in reference.Keys)
            {
                string keyDtId = await KeyExists(key);
                if (keyDtId == null)
                {
                    // Create key
                    var keyTwinData = new BasicDigitalTwin();
                    keyTwinData.Metadata.ModelId = ADTAASOntology.MODEL_KEY;
                    keyTwinData.Id = $"{ADTAASOntology.DTIDMap[ADTAASOntology.MODEL_KEY]["dtId"]}{Guid.NewGuid()}";
                    keyTwinData.Contents.Add("key", key.type);
                    keyTwinData.Contents.Add("value", key.value);
                    keyTwinData.Contents.Add("idType", URITOIRI(key.idType));
                    await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(keyTwinData.Id, keyTwinData);
                    processInfo.Result.DTInstances.Add(new Tuple<string, string>(keyTwinData.Id, keyTwinData.Metadata.ModelId));

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
    }
}
