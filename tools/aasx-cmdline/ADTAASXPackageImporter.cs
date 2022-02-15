using AAS.AASX.CmdLine.ADT;
using AdminShellNS;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;
using File = AdminShellNS.AdminShellV20.File;

namespace AAS.AASX.CmdLine.Import.ADT
{
    public class ADTAASXPackageImporter : AbstractADTCommand, IAASXImporter
    {
        
        public ADTAASXPackageImporter(DigitalTwinsClient adtClient, ILogger<ADTAASXPackageImporter> logger, IAASRepo repo) 
            : base(adtClient, logger, repo)
        {
        }

        public async Task<ImportResult> ImportFromPackageFile(string packageFilePath, ImportContext processInfo)
        {
            if (packageFilePath == null)
                throw new ArgumentNullException(nameof(packageFilePath));

            if (!System.IO.File.Exists(packageFilePath))
            {
                throw new ArgumentException($"AASX package file at '{packageFilePath}' doesn't exist");
            }

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

            BasicDigitalTwinComponent assetKind = new BasicDigitalTwinComponent();
            assetKind.Contents.Add("kind", submodel.kind.kind);
            subModelTwinData.Contents.Add("kind", assetKind);

            await DoCreateOrReplaceDigitalTwinAsync(subModelTwinData, processInfo);

            if (submodel.GetQualifiers() != null && submodel.GetQualifiers().Any())
                await AddQualifiableRelations(subModelTwinData, submodel.GetQualifiers(), processInfo);

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

            // Create Submodel elements
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
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(MultiLanguageProperty))
            {
                return await ImportMultiLanguageProperty(submodelElementWrapper.GetAs<MultiLanguageProperty>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(AdminShellV20.Range))
            {
                return await ImportRange(submodelElementWrapper.GetAs<AdminShellV20.Range>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(File))
            {
                return await ImportFile(submodelElementWrapper.GetAs<File>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(Blob))
            {
                return await ImportBlob(submodelElementWrapper.GetAs<Blob>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(ReferenceElement))
            {
                return await ImportReferenceElement(submodelElementWrapper.GetAs<ReferenceElement>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(Capability))
            {
                return await ImportCapability(submodelElementWrapper.GetAs<Capability>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(RelationshipElement))
            {
                return await ImportRelationshipElement(submodelElementWrapper.GetAs<RelationshipElement>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(AnnotatedRelationshipElement))
            {
                return await ImportAnnotatedRelationshipElement(submodelElementWrapper.GetAs<AnnotatedRelationshipElement>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(AdminShellV20.Operation))
            {
                return await ImportOperation(submodelElementWrapper.GetAs<AdminShellV20.Operation>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(BasicEvent))
            {
                return await ImportBasicEvent(submodelElementWrapper.GetAs<BasicEvent>(), processInfo);
            }
            else if (submodelElementWrapper.submodelElement.GetType() == typeof(Entity))
            {
                return await ImportEntity(submodelElementWrapper.GetAs<Entity>(), processInfo);
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

        private async Task<string> ImportReferenceElement(ReferenceElement refElement, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing reference element '{refElement.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_REFERENCEELEMENT);

            AddDataElementAttributes(twinData, refElement);

            // Reference keys
            for(int i = 1; i <= 8; i++)
            {
                var keyEntry = new BasicDigitalTwinComponent();
                twinData.Contents.Add($"key{i}", keyEntry);
                if (refElement != null && refElement.value != null 
                    && refElement.value.Keys != null && refElement.value.Keys.Count >= i)
                {
                    var key = refElement.value.Keys[i - 1];
                    keyEntry.Contents.Add("key", key.type);
                    keyEntry.Contents.Add("value", key.value);
                    keyEntry.Contents.Add("idType", AASUtils.URITOIRI(key.idType));
                }
            }

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddDataElementRelationships(twinData, refElement, processInfo);

            return twinData.Id;
        }

        private async Task<string> ImportCapability(Capability capability, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing capability '{capability.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_CAPABILITY);

            AddSubmodelElementAttributes(twinData, capability);
            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddSubmodelElementRelationships(twinData, capability, processInfo);

            return twinData.Id;
        }

        private async Task<string> ImportBasicEvent(BasicEvent basicEvent, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing basic event '{basicEvent.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_BASICEVENT);

            AddSubmodelElementAttributes(twinData, basicEvent);
            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddSubmodelElementRelationships(twinData, basicEvent, processInfo);

            string observedTwinId = await FindTwinForReference(basicEvent.observed);
            if (!String.IsNullOrEmpty(observedTwinId))
            {
                await DoCreateOrReplaceRelationshipAsync(twinData, "observed", observedTwinId, processInfo);
            }

            return twinData.Id;
        }

        private async Task<string> ImportEntity(Entity entity, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing entity '{entity.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_ENTITY);

            AddSubmodelElementAttributes(twinData, entity);
            twinData.Contents.Add("entityType", entity.entityType);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddSubmodelElementRelationships(twinData, entity, processInfo);

            if (entity.statements != null)
            {
                foreach (var statement in entity.statements)
                {
                    string statTwinId = await ImportSubmodelElement(statement, twinData, processInfo);
                    await DoCreateOrReplaceRelationshipAsync(twinData, "statement", statTwinId, processInfo);
                }
            }

            // TODO: Version 3 has more detailled infos compared to the V2 schema. Therefore no specificAssetId entry
            if (entity.assetRef != null)
            {
                await AddReference(twinData, entity.assetRef, "globalAssetId", processInfo);
            }

            return twinData.Id;
        }

        public async Task<string> ImportRelationshipElement(RelationshipElement relElement, ImportContext processInfo = null)
        {
            if (relElement == null)
                throw new ArgumentNullException(nameof(relElement));

            _logger.LogInformation($"Now importing relationship element '{relElement.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_RELATIONSHIPELEMENT);

            AddRelationshipElementAttributes(twinData, relElement);
            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddRelationshipElementRelationships(twinData, relElement, processInfo);

            return twinData.Id;
        }

        private void AddRelationshipElementAttributes(BasicDigitalTwin twinData, RelationshipElement relElement)
        {
            AddSubmodelElementAttributes(twinData, relElement);
        }

        private async Task AddRelationshipElementRelationships(BasicDigitalTwin twinData, RelationshipElement relElement, ImportContext processInfo = null)
        {
            await AddSubmodelElementRelationships(twinData, relElement, processInfo);

            // Create relationship to first referable
            var firstTwin = await FindTwinForReference(relElement.first);
            if (firstTwin != null)
                await DoCreateOrReplaceRelationshipAsync(twinData, "first", firstTwin);
            else
            {
                _logger.LogError($"Creating relationship to first element didn't work.");
            }

            // Create relationship to second referable
            var secondTwin = await FindTwinForReference(relElement.second);
            if (secondTwin != null)
                await DoCreateOrReplaceRelationshipAsync(twinData, "second", secondTwin);
            else
            {
                _logger.LogError($"Creating relationship to second element didn't work.");
            }
        }

        public async Task<string> ImportAnnotatedRelationshipElement(AnnotatedRelationshipElement relElement, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing annotated relationship element '{relElement.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_ANNOTATEDRELATIONSHIPELEMENT);

            AddRelationshipElementAttributes(twinData, relElement);
            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddRelationshipElementRelationships(twinData, relElement, processInfo);

            // Created annotation relationship
            if (relElement.annotations != null && relElement.annotations.Any())
            {
                foreach (var annotation in relElement.annotations)
                {
                    // Create DataElement and relationship
                    string dataElementTwinId = await ImportSubmodelElement(annotation, twinData, processInfo);
                    if (dataElementTwinId != null)
                    {
                        await DoCreateOrReplaceRelationshipAsync(twinData, "annotation", dataElementTwinId, processInfo);
                    }
                }
            }

            return twinData.Id;
        }

        private async Task<string> ImportMultiLanguageProperty(MultiLanguageProperty property, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing mulit language property '{property.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_MULTILANGUAGEPROPERTY);

            AddDataElementAttributes(twinData, property);

            if (property.value != null)
            {
                Dictionary<string,string> entries = new Dictionary<string,string>();
                foreach (var item in property.value.langString)
                {
                    entries.Add(item.lang, item.str);
                }

                if (entries.Count > 0) {
                    var langStringSet = new BasicDigitalTwinComponent();
                    langStringSet.Contents.Add("langString", entries);
                    twinData.Contents.Add("value", langStringSet);
                }
            }

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            if (property.valueId != null)
                await AddReference(twinData, property.valueId, "valueId", processInfo);

            await AddDataElementRelationships(twinData, property, processInfo);

            return twinData.Id;
        }

        private async Task<string> ImportRange(AdminShellV20.Range rangeProp, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing range '{rangeProp.idShort}'");

            // Start by creating a twin for the Property
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_RANGE);

            AddDataElementAttributes(twinData, rangeProp);

            if (rangeProp.valueType != null)
                twinData.Contents.Add("valueType", rangeProp.valueType);
            if (rangeProp.min != null)
                twinData.Contents.Add("min", rangeProp.min);
            if (rangeProp.max != null)
                twinData.Contents.Add("max", rangeProp.max);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddDataElementRelationships(twinData, rangeProp, processInfo);

            return twinData.Id;
        }

        private async Task<string> ImportOperation(AdminShellV20.Operation operation, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing operation '{operation.idShort}'");

            // Start by creating a twin for the Operation
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_OPERATION);

            AddSubmodelElementAttributes(twinData, operation);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddSubmodelElementRelationships(twinData, operation, processInfo);

            // Create operation variable relationships
            if (operation.inputVariable != null && operation.inputVariable.Any())
            {
                foreach(var inputVariable in operation.inputVariable)
                {
                    string opVariableTwinId = await ImportOperationVariable(inputVariable, processInfo);
                    await DoCreateOrReplaceRelationshipAsync(twinData, "inputVariable", opVariableTwinId, processInfo);
                }
            }
            if (operation.outputVariable != null && operation.outputVariable.Any())
            {
                foreach (var outputVariable in operation.outputVariable)
                {
                    string opVariableTwinId = await ImportOperationVariable(outputVariable, processInfo);
                    await DoCreateOrReplaceRelationshipAsync(twinData, "outputVariable", opVariableTwinId, processInfo);
                }
            }
            if (operation.inoutputVariable != null && operation.inoutputVariable.Any())
            {
                foreach (var inoutputVariable in operation.inoutputVariable)
                {
                    string opVariableTwinId = await ImportOperationVariable(inoutputVariable, processInfo);
                    await DoCreateOrReplaceRelationshipAsync(twinData, "inoutputVariable", opVariableTwinId, processInfo);
                }
            }

            return twinData.Id;
        }

        private async Task<string> ImportOperationVariable(OperationVariable operationVariable, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing operation variable {operationVariable}");

            // Start by creating a twin for the Operation
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_OPERATIONVARIABLE);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            if (operationVariable.value != null)
            {
                string valueTwinId = await ImportSubmodelElement(operationVariable.value, twinData, processInfo);
                await DoCreateOrReplaceRelationshipAsync(twinData, "value", valueTwinId, processInfo);
            }

            return twinData?.Id;
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

            if (submodelElement.GetQualifiers() != null && submodelElement.GetQualifiers().Any())
                await AddQualifiableRelations(twinData, submodelElement.GetQualifiers(), processInfo);
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

        private async Task<string> ImportBlob(Blob blob, ImportContext processInfo)
        {
            _logger.LogInformation($"Now importing blob '{blob.idShort}'");

            // Start by creating a twin for the Asset
            var twinData = CreateTwinForModel(ADTAASOntology.MODEL_BLOB);

            AddDataElementAttributes(twinData, blob);

            if (blob.mimeType != null)
                twinData.Contents.Add("mimeType", blob.mimeType);
            // Attention: ADT has a 4k limit for strings
            if (blob.value != null)
                twinData.Contents.Add("value", blob.value);

            await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

            await AddDataElementRelationships(twinData, blob, processInfo);

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
            AddIdentifiableAttributes(twinData, conceptDescription);

            if ( conceptDescription.IEC61360Content != null && conceptDescription.IEC61360Content.shortName != null )
            {
                var displayNameComp = twinData.Contents["displayName"] as BasicDigitalTwinComponent;
                if (displayNameComp != null)
                {
                    var entries = displayNameComp.Contents.ContainsKey("langString") ? displayNameComp.Contents["langString"] as Dictionary<string, string> : null;
                    if (entries == null)
                    {
                        entries = new Dictionary<string, string>();
                        displayNameComp.Contents.Add("langString", entries);
                    }
                    entries.Add(LangStr.LANG_DEFAULT.ToUpper(), conceptDescription.IEC61360Content.shortName.GetDefaultStr());
                }
            }

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
                    dsTwinData.Contents.Add("preferredName", CreateLangStringSetComponent(content.preferredName));
                }
                if (content.shortName != null)
                {
                    dsTwinData.Contents.Add("shortName", CreateLangStringSetComponent(content.shortName));
                }
                if (!string.IsNullOrEmpty(content.unit))
                    dsTwinData.Contents.Add("unit", content.unit);
                if (!string.IsNullOrEmpty(content.sourceOfDefinition))
                    dsTwinData.Contents.Add("sourceOfDefinition", content.sourceOfDefinition);
                if (!string.IsNullOrEmpty(content.symbol))
                    dsTwinData.Contents.Add("symbol", content.symbol);
                if (!string.IsNullOrEmpty(content.dataType))
                    dsTwinData.Contents.Add("dataType", content.dataType);
                if (content.definition != null)
                    dsTwinData.Contents.Add("definition", CreateLangStringSetComponent(content.definition));
                if (!string.IsNullOrEmpty(content.valueFormat))
                    dsTwinData.Contents.Add("valueFormat", content.valueFormat);
                // TODO: valueList
                // TODO: value
                // TODO: levelType

                await DoCreateOrReplaceDigitalTwinAsync(dsTwinData, processInfo);

                // Create related unitId
                if (content.unitId != null)
                    await AddReference(dsTwinData, content.unitId.Keys, "unitId", processInfo);

                // Create relationship to 
                await DoCreateOrReplaceRelationshipAsync(twinData, "dataSpecification", dsTwinData.Id);
            }
        }

        private static BasicDigitalTwinComponent CreateLangStringSetComponent(LangStringSetIEC61360 langStrs)
        {
            Dictionary<string, string> entries = new Dictionary<string, string>();
            foreach (var item in langStrs)
            {
                entries.Add(item.lang, item.str);
            }

            if (entries.Count > 0)
            {
                var langStringSet = new BasicDigitalTwinComponent();
                langStringSet.Contents.Add("langString", entries);
                
                return langStringSet;
            }
            else
                return null;
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
            if (reference != null && reference.Keys != null && reference.Keys.Count > 0)
                await AddReference(twinData, reference.Keys, relationshipName, processInfo);
        }

        private async Task AddReference(BasicDigitalTwin twinData, List<Key> forKeys, string relationshipName, ImportContext processInfo)
        {
            if (forKeys != null && forKeys.Count > 0)
                await CreateReferenceTwin(twinData, forKeys, relationshipName, processInfo);
        }

        private async Task<BasicDigitalTwin> CreateReferenceTwin(BasicDigitalTwin twinData, List<Key> forKeys, string relationshipName, ImportContext processInfo)
        {
            // Create Reference twin
            var refTwinData = CreateTwinForModel(ADTAASOntology.MODEL_REFERENCE);

            if (forKeys != null && forKeys.Count > 0)
            {
                int count = 0;
                foreach (var key in forKeys)
                {
                    count++;
                    if (count <= 8) { 
                        string keyPropName = $"key{count}";
                        var keyTwinData = new BasicDigitalTwinComponent();
                        keyTwinData.Contents.Add("key", key.type);
                        keyTwinData.Contents.Add("value", key.value);
                        keyTwinData.Contents.Add("idType", AASUtils.URITOIRI(key.idType));
                        refTwinData.Contents.Add(keyPropName, keyTwinData);
                    }
                    else
                    {
                        _logger.LogError($"Reference contains more than the maximum 8 keys supported. {forKeys}");
                    }
                }
                for(int i = count+1; i < 9; i++)
                {
                    refTwinData.Contents.Add($"key{i}", new BasicDigitalTwinComponent());
                }
            } 
            else
            {
                for(int i = 1; i <= 8; i++)
                {
                    refTwinData.Contents.Add($"key{i}", new BasicDigitalTwinComponent());
                }
            }

            await DoCreateOrReplaceDigitalTwinAsync(refTwinData, processInfo);

            // Create relationship between source twin and Reference twin
            await DoCreateOrReplaceRelationshipAsync(twinData, relationshipName, refTwinData.Id);

            return refTwinData;
        }

        private void AddReferableAttributes(BasicDigitalTwin twinData, Referable referable)
        {
            if (!string.IsNullOrEmpty(referable.idShort))
                twinData.Contents.Add("idShort", referable.idShort);

            twinData.Contents.Add("displayName", new BasicDigitalTwinComponent());

            if (!string.IsNullOrEmpty(referable.category))
                twinData.Contents.Add("category", referable.category);

            if (referable.description != null)
            {
                Dictionary<string, string> descEntries = new Dictionary<string, string>();
                foreach (var desc in referable.description.langString)
                    descEntries.Add(desc.lang, desc.str);

                if (descEntries.Count > 0)
                {
                    var descTwinData = new BasicDigitalTwinComponent();
                    descTwinData.Contents.Add("langString", descEntries);
                    twinData.Contents.Add("description", descTwinData);
                }
                else
                    twinData.Contents.Add("description", new BasicDigitalTwinComponent());
            } else
                twinData.Contents.Add("description", new BasicDigitalTwinComponent());

            twinData.Contents.Add("tags", CreateStandardTagsForImport());
        }

        private void AddIdentifiableAttributes(BasicDigitalTwin twinData, Identifiable identifiable)
        {
            // Referable attributes
            AddReferableAttributes(twinData, identifiable);

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

        private async Task AddQualifiableRelations(BasicDigitalTwin twinData, QualifierCollection qualifiers, ImportContext processInfo)
        {
            // TODO: V3.0 has Formulas beside Qualifiers

            _logger.LogInformation($"Adding qualifiers for twin '{twinData.Id}'");

            if (qualifiers != null && qualifiers.Count > 0)
            {
                foreach (Qualifier qualifier in qualifiers)
                {
                    _logger.LogInformation($"Now importing qualifier '{qualifier.valueId}'");

                    var constraintTwinData = CreateTwinForModel(ADTAASOntology.MODEL_QUALIFIER);
                    if (qualifier.type != null)
                        constraintTwinData.Contents.Add("type", qualifier.type);
                    if (qualifier.valueType != null)
                        constraintTwinData.Contents.Add("valueType", qualifier.valueType);
                    if (qualifier.value != null)
                        constraintTwinData.Contents.Add("value", qualifier.value);
                    await DoCreateOrReplaceDigitalTwinAsync(twinData, processInfo);

                    if (qualifier.valueId != null)
                        await AddReference(constraintTwinData, qualifier.valueId, "valueId", processInfo);

                    // Create semantic Id
                    if (qualifier.semanticId != null)
                    {
                        await AddReference(constraintTwinData, qualifier.semanticId, "semanticId", processInfo);
                    }

                    await DoCreateOrReplaceRelationshipAsync(twinData, "qualifier", constraintTwinData.Id);
                }
            }
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

        private BasicDigitalTwinComponent CreateStandardTagsForImport()
        {
            var tagsComponent = new BasicDigitalTwinComponent();

            tagsComponent.Contents.Add("markers", new Dictionary<string, bool>() { { "import", true} });

            return tagsComponent;
        }
    }
}
