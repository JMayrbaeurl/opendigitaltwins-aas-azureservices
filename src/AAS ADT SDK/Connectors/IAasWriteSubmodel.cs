using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT;

public interface IAasWriteSubmodel
{
    Task CreateSubmodel(Submodel submodel);
    Task CreateSubmodelElementForSubmodel(ISubmodelElement submodelElement, string submodelTwinId);
}