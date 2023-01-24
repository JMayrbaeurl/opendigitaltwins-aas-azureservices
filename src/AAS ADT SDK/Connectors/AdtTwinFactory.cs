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

        private BasicDigitalTwin _twin;
        private readonly ILogger<AdtTwinFactory> _logger;


        public AdtTwinFactory(ILogger<AdtTwinFactory> logger)
        {
            _logger = logger;
        }

        public BasicDigitalTwin GetTwin(ISubmodelElement submodelElement)
        {
            _twin = new BasicDigitalTwin();

            AddReferableValues(submodelElement);
            AddKind(submodelElement);
            if (submodelElement is SubmodelElementCollection)
            {
                AddAdtModelAndId(AdtAasOntology.MODEL_SUBMODELELEMENTCOLLECTION);
            }

            else if (submodelElement is Property)
            {
                AddAdtModelAndId(AdtAasOntology.MODEL_PROPERTY);
                AddPropertyValues((Property)submodelElement);
            }

            else if (submodelElement is File)
            {
                AddAdtModelAndId(AdtAasOntology.MODEL_FILE);
                AddFileValues((File)submodelElement);
            }
            else
            {
                throw new ArgumentException($"SubmodelElement of type {submodelElement.GetType()} is not supported");
            }
            return _twin;
        }

        public BasicDigitalTwin GetTwin(Reference reference)
        {
            _twin = new BasicDigitalTwin();

            AddAdtModelAndId(AdtAasOntology.MODEL_REFERENCE);
            _twin.Contents.Add("type", reference.Type.ToString());
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
                        _twin.Contents.Add(keyPropName, keyTwinData);
                    }
                    else
                    {
                        _logger.LogError($"Reference contains more than the maximum 8 keys supported. {reference.Keys}");
                    }
                }

                for (int i = count + 1; i < 9; i++)
                {
                    _twin.Contents.Add($"key{i}", new BasicDigitalTwinComponent());
                }
            }
            else
            {
                for (int i = 1; i <= 8; i++)
                {
                    _twin.Contents.Add($"key{i}", new BasicDigitalTwinComponent());
                }
            }
            return _twin;
        }


        public BasicDigitalTwin GetTwin(IDataSpecificationContent content)
        {
            _twin = new BasicDigitalTwin();

            if (content is DataSpecificationIec61360)
            {
                AddAdtModelAndId(AdtAasOntology.MODEL_DATASPECIEC61360);
                AddDataSpecificationContentIec61360Values((DataSpecificationIec61360)content);
            }
            else
            {
                throw new ArgumentException("DataSpecificationContent of type '{typeof(content)}' is not supported");
            }

            return _twin;
        }

        public BasicDigitalTwin GetTwin(Qualifier qualifier)
        {
            _twin = new BasicDigitalTwin();

            AddAdtModelAndId(AdtAasOntology.MODEL_QUALIFIER);
            _twin.Contents.Add("type", qualifier.Type);
            _twin.Contents.Add("valueType", qualifier.ValueType.ToString());
            if (qualifier.Value != null)
                _twin.Contents.Add("value", qualifier.Value);
            if (qualifier.Kind != null )
            {
                _twin.Contents.Add("kind",qualifier.Kind.ToString());
            }
            return _twin;
        }

        public BasicDigitalTwin GetTwin(Submodel submodel)
        {
            _twin = new BasicDigitalTwin();

            AddAdtModelAndId(AdtAasOntology.MODEL_SUBMODEL);

            AddKind(submodel);
            AddIdentifiableValues(submodel);

            return _twin;
        }


        private void AddDataSpecificationContentIec61360Values(DataSpecificationIec61360 content)
        {
            if (content.PreferredName != null)
                _twin.Contents.Add("preferredName", Convert(content.PreferredName));
            if (content.ShortName != null)
                _twin.Contents.Add("shortName", Convert(content.ShortName));
            if (!string.IsNullOrEmpty(content.Unit))
                _twin.Contents.Add("unit", content.Unit);
            if (!string.IsNullOrEmpty(content.SourceOfDefinition))
                _twin.Contents.Add("sourceOfDefinition", content.SourceOfDefinition);
            if (!string.IsNullOrEmpty(content.Symbol))
                _twin.Contents.Add("symbol", content.Symbol);
            if (content.DataType != null)
                _twin.Contents.Add("dataType", content.DataType.ToString());
            if (content.Definition != null)
                _twin.Contents.Add("definition", Convert(content.Definition));
            if (!string.IsNullOrEmpty(content.ValueFormat))
                _twin.Contents.Add("valueFormat", content.ValueFormat);
            // TODO: Implement Value List
            //if (content.ValueList != null)
            //    _twin.Contents.Add("valueList", content.ValueList);
            if (content.Value != null)
                _twin.Contents.Add("value", content.Value);
            if (content.LevelType != null)
                _twin.Contents.Add("levelType", content.LevelType.ToString());
        }

        private void AddAdtModelAndId(string modelName)
        {
            _twin.Metadata.ModelId = modelName;
            // TODO: loosen coupling between AdtAasOntology and this Factory
            _twin.Id = $"{AdtAasOntology.DTIDMap[modelName]["dtId"]}{Guid.NewGuid()}";
        }

        private void AddIdentifiableValues(IIdentifiable identifiable)
        {
            AddReferableValues(identifiable);
            
            if (identifiable.Id != null)
            {
                _twin.Contents.Add("id", identifiable.Id);
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

            _twin.Contents.Add("administration", admin);
        }


        private void AddReferableValues(IReferable referable)
        {
            if (!string.IsNullOrEmpty(referable.IdShort))
                _twin.Contents.Add("idShort", referable.IdShort);

            if (!string.IsNullOrEmpty(referable.Category))
                _twin.Contents.Add("category", referable.Category);

            if (!string.IsNullOrEmpty(referable.Checksum))
                _twin.Contents.Add("checksum", referable.Checksum);

            _twin.Contents.Add("description", Convert(referable.Description));
            _twin.Contents.Add("displayName", Convert(referable.DisplayName));
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

        private void AddKind(IHasKind objectWithKind)
        {
            BasicDigitalTwinComponent kind = new BasicDigitalTwinComponent();
            if (objectWithKind.Kind != null)
            {
                kind.Contents.Add("kind", objectWithKind.Kind.ToString());
            }

            _twin.Contents.Add("kind", kind);
        }

        private void AddPropertyValues(Property property)
        {
            _twin.Contents.Add("valueType", property.ValueType.ToString());
            if (property.Value != null)
            {
                _twin.Contents.Add("value", property.Value);
            }

        }

        private void AddFileValues(File file)
        {
            _twin.Contents.Add("contentType", file.ContentType);
            if (file.Value != null)
            {
                _twin.Contents.Add("value", file.Value);
            }
        }

    }


}
