using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT
{
    public class AdtTwinFactory : IAdtTwinFactory
    {

        private BasicDigitalTwin _twin;

        public AdtTwinFactory()
        {
            _twin = new BasicDigitalTwin();
        }

        public BasicDigitalTwin GetTwin(ISubmodelElement submodelElement, string modelName)
        {
            AddAdtModelAndId(modelName);
            AddSubmodelElementValues(submodelElement);
            return _twin;
        }

        private void AddAdtModelAndId(string modelName)
        {
            _twin.Metadata.ModelId = modelName;
            // TODO: loosen coupling between AdtAasOntology and this Factory
            _twin.Id = $"{AdtAasOntology.DTIDMap[modelName]["dtId"]}{Guid.NewGuid()}";
        }

        private void AddSubmodelElementValues(ISubmodelElement submodelElement)
        {
            AddReferableValues(submodelElement);
            AddKind(submodelElement);
            if (submodelElement is SubmodelElementCollection)
            {
                // nothing to do
            }

            if (submodelElement is Property)
            {
                AddPropertyValues((Property)submodelElement);
            }

            if (submodelElement is File)
            {
                AddFileValues((File)submodelElement);
            }

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
