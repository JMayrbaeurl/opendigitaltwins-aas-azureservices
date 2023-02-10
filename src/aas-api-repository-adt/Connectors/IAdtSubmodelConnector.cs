using System.Text.Json.Nodes;
using AAS.API.Repository.Adt.Models;

namespace AAS.API.Repository.Adt;

public interface IAdtSubmodelConnector
{
    Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId);
    AdtConcreteAasInformation DeserializeAdtResponse(string relationship, JsonNode dataTwin, AdtConcreteAasInformation information);

    Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAllSubmodelElementCollectionInformation(
        string twinId);

    public Task<DefinitionsAndSemantics> GetAllDescriptionsForSubmodelElements(string rootTwinId);
}