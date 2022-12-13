using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtModels.AdtModels;
using Azure.DigitalTwins.Core;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtConcreteAasInformation
    {
        public List<AdtDataSpecification> dataSpecifications =
            new List<AdtDataSpecification>();

        public AdtReference semanticId = new AdtReference();

        public List<AdtReference> supplementalSemanticId = new List<AdtReference>();
    }

    public abstract class AdtGeneralAasInformation<T> where T : AdtBase, new()
    {
        public AdtGeneralAasInformation()
        {
            RootElement = new T();
        }
        public T RootElement { get; set; }

        public AdtConcreteAasInformation ConcreteAasInformation = new AdtConcreteAasInformation();
        public DefinitionsAndSemantic definitionsAndSemantic = new DefinitionsAndSemantic();

    }

    public class DefinitionsAndSemantic
    {
        public Dictionary<string, AdtReference> References = new Dictionary<string, AdtReference>();

        public Dictionary<string, AdtDataSpecificationIEC61360> Iec61360s =
            new Dictionary<string, AdtDataSpecificationIEC61360>();

        public Dictionary<string, AdtConceptDescription> ConceptDescriptions =
            new Dictionary<string, AdtConceptDescription>();

        public Dictionary<string, List<BasicRelationship>> Relationships = new Dictionary<string, List<BasicRelationship>>();

        public Dictionary<string, List<AdtConceptDescription>> conceptDescriptions =
            new Dictionary<string, List<AdtConceptDescription>>();
    }

    public abstract class AdtSubmodelAndSmcInformation<T> : AdtGeneralAasInformation<T> where T : AdtBase, new()
    {
        public List<AdtSubmodelElementCollectionInformation> smeCollections = new List<AdtSubmodelElementCollectionInformation>();
        public List<AdtProperty> properties = new List<AdtProperty>();
        public List<AdtFile> files = new List<AdtFile>();
    }

    public class AdtSubmodelInformation : AdtSubmodelAndSmcInformation<AdtSubmodel>
    {
    }

    public class AdtSubmodelElementCollectionInformation :
        AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>
    {
    }
}
