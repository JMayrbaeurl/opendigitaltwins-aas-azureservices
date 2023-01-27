using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT;

public interface IAasWriteAssetAdministrationShell
{
    Task CreateShell(AssetAdministrationShell shell);
    Task CreateSubmodelReference(string shellTwinId, string submodelTwinId);
}