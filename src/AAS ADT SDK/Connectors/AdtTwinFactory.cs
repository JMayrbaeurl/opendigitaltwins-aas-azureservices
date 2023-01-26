using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection.Metadata;

namespace AAS.ADT
{
    public class AdtTwinFactory : IAdtTwinFactory
    {

        private readonly ILogger<AdtTwinFactory> _logger;


        public AdtTwinFactory(ILogger<AdtTwinFactory> logger)
        {
            _logger = logger;
        }

        public BasicDigitalTwin GetTwin(AssetAdministrationShell shell)
        {
            var twin = new BasicDigitalTwin();
            AddAdtModelAndId(twin, AdtAasOntology.MODEL_SHELL);
            AddIdentifiableValues(twin, shell);

            var assetInfoShort = new BasicDigitalTwinComponent();
            assetInfoShort.Contents.Add("assetKind", shell.AssetInformation.AssetKind.ToString());
            if (shell.AssetInformation.GlobalAssetId!= null)
            {
                assetInfoShort.Contents.Add("globalAssetId", Serialize(shell.AssetInformation.GlobalAssetId));
            }
            
            var specificAssetIds = shell.AssetInformation.SpecificAssetIds;
            if (specificAssetIds!= null && specificAssetIds.Count > 0)
            {
                assetInfoShort.Contents.Add("specificAssetId",
                SerializeSpecificAssetIds(specificAssetIds));
            }

            if (shell.AssetInformation.DefaultThumbnail != null && shell.AssetInformation.DefaultThumbnail.Path != "")
            {
                assetInfoShort.Contents.Add("defaultThumbnailpath", shell.AssetInformation.DefaultThumbnail.Path);
            }
            

            twin.Contents.Add("assetInformationShort", assetInfoShort);
            return twin;
        }

        public BasicDigitalTwin GetTwin(AssetInformation assetInformation)
        {
            var twin = new BasicDigitalTwin();

            AddAdtModelAndId(twin,AdtAasOntology.MODEL_ASSETINFORMATION);
            
            var assetKind = new BasicDigitalTwinComponent();
            assetKind.Contents.Add("assetKind",assetInformation.AssetKind.ToString());
            twin.Contents.Add("assetKind",assetKind);
            return twin;
        }

        private string Serialize(Reference reference)
        {
            return reference.Type == ReferenceTypes.GlobalReference ? 
                $"(GlobalReference){reference.Keys[0].Value}" : 
                $"({reference.Keys[0].Type}){reference.Keys[0].Value}";
        }

        private string SerializeSpecificAssetIds(List<SpecificAssetId> specificAssetIds)
        {
            var result = "";
            foreach (var specificAssetId in specificAssetIds)
            {
                result += $"({specificAssetId.Name}){specificAssetId.Value}, ";
            }

            return result.Remove(result.Length - 2);
        }

        public BasicDigitalTwin GetTwin(ISubmodelElement submodelElement)
        {
            var twin = new BasicDigitalTwin();

            AddReferableValues(twin, submodelElement);
            AddKind(twin, submodelElement);
            if (submodelElement is SubmodelElementCollection)
            {
                AddAdtModelAndId(twin, AdtAasOntology.MODEL_SUBMODELELEMENTCOLLECTION);
            }

            else if (submodelElement is Property element)
            {
                AddAdtModelAndId(twin, AdtAasOntology.MODEL_PROPERTY);
                AddPropertyValues(twin, element);
            }

            else if (submodelElement is File file)
            {
                AddAdtModelAndId(twin,AdtAasOntology.MODEL_FILE);
                AddFileValues(twin, file);
            }
            else
            {
                throw new ArgumentException($"SubmodelElement of type {submodelElement.GetType()} is not supported");
            }
            return twin;
        }

        public BasicDigitalTwin GetTwin(Reference reference)
        {
            var twin = new BasicDigitalTwin();

            AddAdtModelAndId(twin, AdtAasOntology.MODEL_REFERENCE);
            twin.Contents.Add("type", reference.Type.ToString());
            if (reference.Keys.Count > 0)
            {
                int count = 0;
                foreach (var key in reference.Keys)
                {
                    count++;
                    if (count <= 8)
                    {
                        string keyPropName = $"key{count}";
                        var keyTwinData = new BasicDigitalTwinComponent();
                        keyTwinData.Contents.Add("type", key.Type);
                        keyTwinData.Contents.Add("value", key.Value);
                        twin.Contents.Add(keyPropName, keyTwinData);
                    }
                    else
                    {
                        _logger.LogError($"Reference contains more than the maximum 8 keys supported. {reference.Keys}");
                    }
                }

                for (int i = count + 1; i < 9; i++)
                {
                    twin.Contents.Add($"key{i}", new BasicDigitalTwinComponent());
                }
            }
            else
            {
                for (int i = 1; i <= 8; i++)
                {
                    twin.Contents.Add($"key{i}", new BasicDigitalTwinComponent());
                }
            }
            return twin;
        }


        public BasicDigitalTwin GetTwin(IDataSpecificationContent content)
        {
            var twin = new BasicDigitalTwin();

            if (content is DataSpecificationIec61360 dataSpecificationIec61360)
            {
                AddAdtModelAndId(twin, AdtAasOntology.MODEL_DATASPECIEC61360);
                AddDataSpecificationContentIec61360Values(twin, dataSpecificationIec61360);
            }
            else
            {
                throw new ArgumentException("DataSpecificationContent of type '{typeof(content)}' is not supported");
            }

            return twin;
        }

        public BasicDigitalTwin GetTwin(Qualifier qualifier)
        {
            var twin = new BasicDigitalTwin();

            AddAdtModelAndId(twin, AdtAasOntology.MODEL_QUALIFIER);
            twin.Contents.Add("type", qualifier.Type);
            twin.Contents.Add("valueType", qualifier.ValueType.ToString());
            if (qualifier.Value != null)
                twin.Contents.Add("value", qualifier.Value);
            if (qualifier.Kind != null)
            {
                twin.Contents.Add("kind", qualifier.Kind.ToString());
            }
            return twin;
        }

        public BasicDigitalTwin GetTwin(Submodel submodel)
        {
            var twin = new BasicDigitalTwin();

            AddAdtModelAndId(twin, AdtAasOntology.MODEL_SUBMODEL);

            AddKind(twin, submodel);
            AddIdentifiableValues(twin, submodel);

            return twin;
        }


        private void AddDataSpecificationContentIec61360Values(BasicDigitalTwin twin, DataSpecificationIec61360 content)
        {
            if (content.PreferredName != null)
                twin.Contents.Add("preferredName", Convert(content.PreferredName));
            if (content.ShortName != null)
                twin.Contents.Add("shortName", Convert(content.ShortName));
            if (!string.IsNullOrEmpty(content.Unit))
                twin.Contents.Add("unit", content.Unit);
            if (!string.IsNullOrEmpty(content.SourceOfDefinition))
                twin.Contents.Add("sourceOfDefinition", content.SourceOfDefinition);
            if (!string.IsNullOrEmpty(content.Symbol))
                twin.Contents.Add("symbol", content.Symbol);
            if (content.DataType != null)
                twin.Contents.Add("dataType", content.DataType.ToString());
            if (content.Definition != null)
                twin.Contents.Add("definition", Convert(content.Definition));
            if (!string.IsNullOrEmpty(content.ValueFormat))
                twin.Contents.Add("valueFormat", content.ValueFormat);
            // TODO: Implement Value List
            //if (content.ValueList != null)
            //    _twin.Contents.Add("valueList", content.ValueList);
            if (content.Value != null)
                twin.Contents.Add("value", content.Value);
            if (content.LevelType != null)
                twin.Contents.Add("levelType", content.LevelType.ToString());
        }

        private void AddAdtModelAndId(BasicDigitalTwin twin,string modelName)
        {
            twin.Metadata.ModelId = modelName;
            // TODO: loosen coupling between AdtAasOntology and this Factory
            twin.Id = $"{AdtAasOntology.DTIDMap[modelName]["dtId"]}{Guid.NewGuid()}";
        }

        private void AddIdentifiableValues(BasicDigitalTwin twin, IIdentifiable identifiable)
        {
            AddReferableValues(twin, identifiable);

            if (identifiable.Id != null)
            {
                twin.Contents.Add("id", identifiable.Id);
            }
            BasicDigitalTwinComponent admin = new BasicDigitalTwinComponent();
            if (identifiable.Administration != null &&
                (identifiable.Administration.Version != null || identifiable.Administration.Revision != null))
            {
                if (!string.IsNullOrEmpty(identifiable.Administration.Version))
                    admin.Contents.Add("version", identifiable.Administration.Version);
                if (!string.IsNullOrEmpty(identifiable.Administration.Revision))
                    admin.Contents.Add("revision", identifiable.Administration.Revision);
            }

            twin.Contents.Add("administration", admin);
        }


        private void AddReferableValues(BasicDigitalTwin twin, IReferable referable)
        {
            if (!string.IsNullOrEmpty(referable.IdShort))
                twin.Contents.Add("idShort", referable.IdShort);

            if (!string.IsNullOrEmpty(referable.Category))
                twin.Contents.Add("category", referable.Category);

            if (!string.IsNullOrEmpty(referable.Checksum))
                twin.Contents.Add("checksum", referable.Checksum);

            twin.Contents.Add("description", Convert(referable.Description));
            twin.Contents.Add("displayName", Convert(referable.DisplayName));
        }

        private BasicDigitalTwinComponent Convert(List<LangString> langStrings)
        {
            var descEntries = new Dictionary<string, string>();
            var descTwinData = new BasicDigitalTwinComponent();
            if (langStrings != null)
            {
                foreach (var langString in langStrings)
                    descEntries.Add(langString.Language, langString.Text);

                if (descEntries.Count > 0)
                {
                    descTwinData.Contents.Add("langString", descEntries);
                }
            }
            return descTwinData;
        }

        private void AddKind(BasicDigitalTwin twin, IHasKind objectWithKind)
        {
            BasicDigitalTwinComponent kind = new BasicDigitalTwinComponent();
            if (objectWithKind.Kind != null)
            {
                kind.Contents.Add("kind", objectWithKind.Kind.ToString());
            }

            twin.Contents.Add("kind", kind);
        }

        private void AddPropertyValues(BasicDigitalTwin twin, Property property)
        {
            twin.Contents.Add("valueType", property.ValueType.ToString());
            if (property.Value != null)
            {
                twin.Contents.Add("value", property.Value);
            }

        }

        private void AddFileValues(BasicDigitalTwin twin, File file)
        {
            twin.Contents.Add("contentType", file.ContentType);
            if (file.Value != null)
            {
                twin.Contents.Add("value", file.Value);
            }
        }

    }


}
