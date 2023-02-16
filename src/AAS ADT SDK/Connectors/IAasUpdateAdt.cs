using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT;

public interface IAasUpdateAdt
{
    Task UpdateFullSubmodel(string submodelTwinId, Submodel submodel);
    Task UpdateFullShell(string shellTwinId, AssetAdministrationShell shell);
}