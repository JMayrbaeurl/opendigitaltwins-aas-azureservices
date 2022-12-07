using System.Collections.Generic;
using AdtModels.AdtModels;

namespace AAS_Services_Support.ADT_Support;

public interface IAdtInteractions
{
    List<string> GetAllAasIds();
    AdtAas GetAdtAasForAasWithId(string aasId);
    List<AdtResponseForAllAasInformation> GetAllInformationForAasWithId(string aasId);
    List<string> GetAllSubmodelTwinIds();
    AdtSubmodel GetAdtSubmodelWithSubmodelId(string submodelId);
    List<AdtSubmodelElement> GetAdtSubmodelElementsFromParentTwinWithId(string adtTwinId);
    }