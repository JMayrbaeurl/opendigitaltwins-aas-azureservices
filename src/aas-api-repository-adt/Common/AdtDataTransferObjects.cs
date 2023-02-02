using System.Text.Json.Nodes;
using AAS.ADT.Models;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt
{

    public class DefinitionsAndSemantics
    {
        public Dictionary<string, AdtReference> References = new();

        public Dictionary<string, AdtDataSpecificationIEC61360> Iec61360s = new();

        public Dictionary<string, AdtConceptDescription> ConceptDescriptions = new();

        public Dictionary<string, List<BasicRelationship>> Relationships = new();
    }

    public class AdtGeneralAasInformation<T> where T : AdtBase, new()
    {
        public AdtGeneralAasInformation()
        {
            rootElement = new T();
        }
        public T rootElement { get; set; }
    }
    
    public class AdtSubmodelElements
    {
        public List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> smeCollections = new();
        public List<AdtProperty> properties = new();
        public List<AdtFile> files = new();
    }


    public class AdtSubmodelAndSmcInformation<T> where T : AdtBase, new()
    {
        public AdtSubmodelAndSmcInformation()
        {
            RootElement = new T();
        }

        public T RootElement { get; set; }

        public DefinitionsAndSemantics DefinitionsAndSemantics = new();
        public AdtSubmodelElements AdtSubmodelElements = new();
    }

    public class AdtAssetAdministrationShellInformation : AdtGeneralAasInformation<AdtAas>
    {
        public List<AdtSubmodel> Submodels = new();
        public AdtAas? DerivedFrom = null;
        public AdtAssetInformation? AssetInformation = null;
    }
}
