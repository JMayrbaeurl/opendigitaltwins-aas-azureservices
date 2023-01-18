using AAS.ADT.Models;

namespace AAS.API.Repository.Adt;

public interface IAdtAasConnector
{
    List<string> GetAllAasIds();
    AdtAas GetAdtAasForAasWithId(string aasId);
    AdtAssetAdministrationShellInformation GetAllInformationForAasWithId(string aasId);
    Task<List<string>> GetAllSubmodelTwinIds();
    string GetTwinIdForElementWithId(string Id);
}