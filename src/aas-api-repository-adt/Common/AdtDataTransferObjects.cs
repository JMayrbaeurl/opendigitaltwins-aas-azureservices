using System.Text.Json.Nodes;
using AAS.ADT.Models;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt
{
    public class AdtConcreteAasInformation
    {
        public List<AdtDataSpecification> dataSpecifications =
            new List<AdtDataSpecification>();

        public AdtReference semanticId = new AdtReference();

        public List<AdtReference> supplementalSemanticId = new List<AdtReference>();
    }

    public class AdtGeneralAasInformation<T> where T : AdtBase, new()
    {
        public AdtGeneralAasInformation()
        {
            RootElement = new T();
        }
        public T RootElement { get; set; }

        public AdtConcreteAasInformation ConcreteAasInformation = new AdtConcreteAasInformation();
        public DefinitionsAndSemantics definitionsAndSemantics = new DefinitionsAndSemantics();
        public List<(JsonNode, string)> relatedTwins = new List<(JsonNode, string)>();
    }

    public class DefinitionsAndSemantics
    {
        public Dictionary<string, AdtReference> References = new Dictionary<string, AdtReference>();

        public Dictionary<string, AdtDataSpecificationIEC61360> Iec61360s =
            new Dictionary<string, AdtDataSpecificationIEC61360>();

        public Dictionary<string, AdtConceptDescription> ConceptDescriptions =
            new Dictionary<string, AdtConceptDescription>();

        public Dictionary<string, List<BasicRelationship>> Relationships = new Dictionary<string, List<BasicRelationship>>();
    }

    public class AdtSubmodelAndSmcInformation<T> : AdtGeneralAasInformation<T> where T : AdtBase, new()
    {
        public List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> smeCollections = new List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>>();
        public List<AdtProperty> properties = new List<AdtProperty>();
        public List<AdtFile> files = new List<AdtFile>();
    }

    public class AdtAssetAdministrationShellInformation : AdtGeneralAasInformation<AdtAas>
    {
        public List<AdtSubmodel> Submodels = new List<AdtSubmodel>();
        public AdtAas DerivedFrom = null;
        public AdtAssetInformation AssetInformation = null;
    }
}
