using AAS.AASX.Support;
using AdminShellNS;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.ADT
{
    public class ADTAASXPackageImporter : AbstractADTCommand, IAASXImporter
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
                                await ImportAsset(asset, processInfo);
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
                                    await DeleteShell(shell);

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
                                await ImportConceptDescription(desc, processInfo);
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
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_SHELL);
            AddIdentifiableAttributes(twinData, shell);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            // Create twins and relationships for embedded data specifications
            if (shell.hasDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, shell.hasDataSpecification, processInfo);
            }

            // Create asset information
            if (shell.assetRef != null)
            {
                TwinRef<Asset> assetRef = processInfo.Result.AASAssets[shell.assetRef.First.idType+ shell.assetRef.First.value];

                var assetInfoTwinData = CreateTwinForModel(ADTAASOntology.MODEL_ASSETINFORMATION);
               
                BasicDigitalTwinComponent assetKind = new BasicDigitalTwinComponent();
                assetKind.Contents.Add("assetKind", assetRef.AASOject.kind.kind);
                assetInfoTwinData.Contents.Add("assetKind", assetKind);

                await DoCreateOrReplaceDigitalTwinAsync(assetInfoTwinData, processInfo);

                // Create relationship from Shell to AssetInfo
                await DoCreateOrReplaceRelationshipAsync(twinData, "assetInformation", assetInfoTwinData.Id);

                // Create relationship from AssetInfo to Asset
                await DoCreateOrReplaceRelationshipAsync(assetInfoTwinData, "globalAssetRef", assetRef.DtId);
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
                        Submodel submodel = FindSubmodelWithRef(submodelRef.First, package);
                        if (submodel != null)
                        {
                            await ImportSubmodelFor(submodel, shell, twinData, processInfo);
                        }
                    }
                }
            }
        }

        private async Task ImportSubmodelFor(Submodel submodel, AdministrationShell shell,
            BasicDigitalTwin shellTwin, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing Submodel '{submodel.idShort}' for shell '{shell.idShort}' into ADT instance");

            // Start by creating a twin for the Submodel
            BasicDigitalTwin subModelTwinData = CreateTwinForModel(ADTAASOntology.MODEL_SUBMODEL);
            
            AddIdentifiableAttributes(subModelTwinData, submodel);
            if (submodel.GetQualifiers() != null && submodel.GetQualifiers().Any())
                AddQualifiableAttributes(subModelTwinData, submodel.GetQualifiers());

            BasicDigitalTwinComponent assetKind = new BasicDigitalTwinComponent();
            assetKind.Contents.Add("kind", submodel.kind.kind);
            subModelTwinData.Contents.Add("kind", assetKind);

            await DoCreateOrReplaceDigitalTwinAsync(subModelTwinData, processInfo);

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
                        await DoCreateOrReplaceRelationshipAsync(subModelTwinData, "submodelElement", submodelElementDtId);
                    }
                }
            }

            // Create relationship between Shell and Submodel
            await DoCreateOrReplaceRelationshipAsync(shellTwin, "submodel", subModelTwinData.Id);
        }

        private async Task<string> ImportSubmodelElement(SubmodelElementWrapper submodelElementWrapper, BasicDigitalTwin subModelTwinData, ImportContext processInfo)
        {
            if (submodelElementWrapper.submodelElement.GetType() == typeof(SubmodelElementCollection))
            {
                return await ImportSubmodelElementCollection(submodelElementWrapper.GetAs<SubmodelElementCollection>(), processInfo);
            } 
            else if (submodelElementWrapper.submodelElement.GetType() == typeof (Property))
            {
                return await ImportProperty(submodelElementWrapper.GetAs<Property>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(File))
            {
                return await ImportFile(submodelElementWrapper.GetAs<File>(), processInfo);
            }

            _logger.LogError($"ImportSubmodelElement called for unsupported SubmodelElement '{submodelElementWrapper.submodelElement.GetType()}'");

            return null;
        }

        private async Task<string> ImportSubmodelElementCollection(SubmodelElementCollection submodelElementCollection, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing submodel element collection '{submodelElementCollection.idShort}'");

            // Start by creating a twin for the Submodel Element collection
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_SUBMODELELEMENTCOLLECTION);

            AddSubmodelElementAttributes(twinData, submodelElementCollection);
            twinData.Contents.Add("ordered", submodelElementCollection.ordered);
            twinData.Contents.Add("allowDuplicates", submodelElementCollection.allowDuplicates);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddSubmodelElementRelationships(twinData, submodelElementCollection, processInfo);

            // Add all submodel elements
            var enumerator = submodelElementCollection.EnumerateChildren();
            foreach (var submodelElement in enumerator)
            {
                string submodelElementDtId = await ImportSubmodelElement(submodelElement, twinData, processInfo);
                if (submodelElementDtId != null)
                {
                    await DoCreateOrReplaceRelationshipAsync(twinData, "value", submodelElementDtId);
                }
            }

            return twinData.Id;
        }

        private async Task<string> ImportProperty(Property property, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing property '{property.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_PROPERTY);

            AddDataElementAttributes(twinData, property);

            if (property.valueType != null)
                twinData.Contents.Add("valueType", property.valueType);
            if (property.value != null)
                twinData.Contents.Add("value", property.value);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            if (property.valueId != null)
                await AddReference(twinData, property.valueId, "valueId", processInfo);

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

        private async Task<string> ImportFile(File fileSpec, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing file '{fileSpec.idShort}'");

            // Start by creating a twin for the Asset
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_FILE);
            
            AddDataElementAttributes(twinData, fileSpec);

            if (fileSpec.mimeType != null)
                twinData.Contents.Add("mimeType", fileSpec.mimeType);
            if (fileSpec.value != null)
                twinData.Contents.Add("value", fileSpec.value);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddDataElementRelationships(twinData, fileSpec, processInfo);

            return twinData.Id;
        }

        private Submodel FindSubmodelWithRef(Key submodelKey, AdminShellPackageEnv package)
        {
            Submodel result = package.AasEnv.Submodels.First<Submodel>(
                submodel => submodel.identification.id == submodelKey.value && submodel.identification.idType == submodelKey.idType );

            return result;
        }

        public async Task ImportAsset(Asset asset, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing Asset '{asset.idShort}' into ADT instance");

            if (await AssetExists(asset))
            {
                _logger.LogInformation($"Skipping creation of twin for Asset '{asset.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Asset
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_ASSET);
            AddIdentifiableAttributes(twinData, asset);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);
            processInfo.Result.AASAssets.Add(asset.identification.idType + asset.identification.id, 
                new TwinRef<Asset>() { DtId = twinData.Id, AASOject = asset});

            // Create twins and relationships for embedded data specifications
            if (asset.hasDataSpecification != null)
            {
                await AddHasDataSpecification(twinData, asset.hasDataSpecification, processInfo);
            }
        }

        public async Task ImportConceptDescription(ConceptDescription conceptDescription, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing Concept description '{conceptDescription.idShort}' into ADT instance");

            if (await ConceptDescriptionExists(conceptDescription))
            {
                _logger.LogInformation($"Skipping creation of twin for Concept description '{conceptDescription.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Concept description
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_CONCEPTDESCRIPTION);
            AddIdentifiableAttributes(twinData, conceptDescription,
                (conceptDescription.IEC61360Content != null && conceptDescription.IEC61360Content.shortName != null) ?
                conceptDescription.IEC61360Content.shortName.GetDefaultStr() : null);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

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
                var dsTwinData = CreateTwinForModel(ADTAASOntology.MODEL_DATASPECIEC61360);

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

                await DoCreateOrReplaceDigitalTwinAsync(dsTwinData, processInfo);

                // Create relationship to 
                await DoCreateOrReplaceRelationshipAsync(twinData, "dataSpecification", dsTwinData.Id);
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
            var refTwinData = CreateTwinForModel(ADTAASOntology.MODEL_REFERENCE);
            await DoCreateOrReplaceDigitalTwinAsync(refTwinData, processInfo);

            // Create relationship between source twin and Reference twin
            await DoCreateOrReplaceRelationshipAsync(twinData, relationshipName, refTwinData.Id);

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
                    var keyTwinData = CreateTwinForModel(ADTAASOntology.MODEL_KEY);
                    keyTwinData.Contents.Add("key", key.type);
                    keyTwinData.Contents.Add("value", key.value);
                    keyTwinData.Contents.Add("idType", URITOIRI(key.idType));

                    await DoCreateOrReplaceDigitalTwinAsync(keyTwinData, processInfo);

                    keyDtId = keyTwinData.Id;
                }

                await DoCreateOrReplaceRelationshipAsync(refTwinData, "key", keyDtId);
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

        private async Task<Response<BasicDigitalTwin>> DoCreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twinData, ImportContext processInfo = null)
        {
            Response<BasicDigitalTwin> result;
            try
            {
                result = await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);

                if (processInfo != null && processInfo.Result != null)
                    processInfo.Result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));
            }
            catch (RequestFailedException ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"Exeception on creating twin with id '{twinData.Id}' and model '{twinData.Metadata.ModelId}': {ex.Message}");
                
                throw new ImportException($"Exeception on creating twin with id '{twinData.Id}' and model '{twinData.Metadata.ModelId}': {ex.Message}", ex);
            }

            return result;
        }

        private BasicDigitalTwin CreateTwinForModel(string modelName)
        {
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = modelName;
            twinData.Id = $"{ADTAASOntology.DTIDMap[modelName]["dtId"]}{Guid.NewGuid()}";

            return twinData;
        }

        private async Task<Response<BasicRelationship>> DoCreateOrReplaceRelationshipAsync(BasicDigitalTwin twinData, string relName, string targetId, ImportContext processInfo = null)
        {
            var relationship = new BasicRelationship
            {
                TargetId = targetId,
                Name = relName
            };

            string relId = $"{twinData.Id}-{relName}->{targetId}";
            Response<BasicRelationship> result;
            try
            {
                result = await dtClient.CreateOrReplaceRelationshipAsync(twinData.Id, relId, relationship);
            }
            catch (RequestFailedException ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"Exeception on creating relationship with name'{relName}' for twin with id '{twinData.Id}' and target id '{targetId}': {ex.Message}");

                throw new ImportException($"Exeception on creating relationship with name'{relName}' for twin with id '{twinData.Id}' and target id '{targetId}': {ex.Message}", ex);
            }

            return result;
        }
    }
}
