using System.Text.Json.Nodes;
using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt;

public interface IAdtSubmodelConnector
{
    Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId);
    AdtConcreteAasInformation DeserializeAdtResponse(string relationship, JsonNode dataTwin, AdtConcreteAasInformation information);

    Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAllSubmodelElementCollectionInformation(
        string twinId);

    public Task<DefinitionsAndSemantics> GetAllDescriptionsForSubmodelElements(string rootTwinId);
    public void CreateProperty(string submodelIdentifier, Property property);
}