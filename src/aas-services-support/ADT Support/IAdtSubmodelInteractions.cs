using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AAS_Services_Support.ADT_Support;

public interface IAdtSubmodelInteractions
{
    Task<AdtSubmodelInformation> GetAllInformationForSubmodelWithTwinId(string twinId);
    AdtConcreteAasInformation DeserializeAdtResponse(string relationship, JsonNode dataTwin, AdtConcreteAasInformation information);

    Task<AdtSubmodelElementCollectionInformation> GetAllSubmodelElementCollectionInformation(
        string twinId);

    public Task<DefinitionsAndSemantic> GetAllDescriptionsForSubmodelElements(string rootTwinId);
}