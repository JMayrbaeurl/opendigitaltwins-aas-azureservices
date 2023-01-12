using AAS.API.Repository.Adt.Models;

namespace AAS.API.Repository.Adt;

public interface IAdtInteractions
{
    List<string> GetAllAasIds();
    AdtAas GetAdtAasForAasWithId(string aasId);
    AdtAssetAdministrationShellInformation GetAllInformationForAasWithId(string aasId);
    Task<List<string>> GetAllSubmodelTwinIds();
    string GetTwinIdForElementWithId(string Id);
}