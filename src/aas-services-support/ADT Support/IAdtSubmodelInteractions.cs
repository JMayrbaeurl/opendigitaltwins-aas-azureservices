using AdtModels.AdtModels;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AAS_Services_Support.ADT_Support;

public interface IAdtSubmodelInteractions
{
    Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId);
    AdtConcreteAasInformation DeserializeAdtResponse(string relationship, JsonNode dataTwin, AdtConcreteAasInformation information);

    Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAllSubmodelElementCollectionInformation(
        string twinId);

    public Task<DefinitionsAndSemantics> GetAllDescriptionsForSubmodelElements(string rootTwinId);
}