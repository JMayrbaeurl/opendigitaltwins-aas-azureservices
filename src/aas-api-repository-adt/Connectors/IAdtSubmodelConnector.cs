using System.Text.Json.Nodes;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt;

public interface IAdtSubmodelConnector
{
    Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId);
}