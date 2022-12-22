using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdtModels.AdtModels;

namespace AAS_Services_Support.ADT_Support;

public interface IAdtInteractions
{
    List<string> GetAllAasIds();
    AdtAas GetAdtAasForAasWithId(string aasId);
    AdtAssetAdministrationShellInformation GetAllInformationForAasWithId(string aasId);
    Task<List<string>> GetAllSubmodelTwinIds();
    AdtSubmodel GetAdtSubmodelWithSubmodelId(string submodelId);
    List<AdtSubmodelElement> GetAdtSubmodelElementsFromParentTwinWithId(string adtTwinId);
    string GetTwinIdForElementWithId(string Id);

    public class AdtException : Exception
    {
        public AdtException(string message) : base(message)
        {
        }

        public AdtException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}