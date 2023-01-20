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

        private BasicDigitalTwin twin;

        public AdtTwinFactory()
        {
            twin = new BasicDigitalTwin();
        }

        public BasicDigitalTwin CreateTwin(ISubmodelElement submodelElement, string modelName)
        {
            AddAdtModelAndId(modelName);
            AddSubmodelElementValues(submodelElement);
            return twin;
        }

        private void AddAdtModelAndId(string modelName)
        {
            twin.Metadata.ModelId = modelName;
            // TODO: loosen coupling between AdtAasOntology and this Factory
            twin.Id = $"{AdtAasOntology.DTIDMap[modelName]["dtId"]}{Guid.NewGuid()}";
        }

        private void AddSubmodelElementValues(ISubmodelElement submodelElement)
        {
            AddReferableValues(submodelElement);
            AddKind(submodelElement);
        }

        private void AddReferableValues(IReferable referable)
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

        private void AddKind(IHasKind objectWithKind)
        {
            BasicDigitalTwinComponent kind = new BasicDigitalTwinComponent();
            if (objectWithKind.Kind != null)
            {
                kind.Contents.Add("kind", objectWithKind.Kind.ToString());
            }

            twin.Contents.Add("kind", kind);
        }
    }


}
