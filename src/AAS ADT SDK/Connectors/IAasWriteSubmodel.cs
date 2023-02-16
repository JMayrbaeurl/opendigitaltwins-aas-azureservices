using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT;

public interface IAasWriteSubmodel
{
    Task<string> CreateSubmodel(Submodel submodel);
    Task CreateSubmodelElementForSubmodel(ISubmodelElement submodelElement, string submodelTwinId);
}