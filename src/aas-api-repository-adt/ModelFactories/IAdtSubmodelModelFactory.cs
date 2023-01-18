using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt;

public interface IAdtSubmodelModelFactory
{
    Task<Submodel> GetSubmodel(AdtSubmodelAndSmcInformation<AdtSubmodel> information);
    void CreateSubmodelElement(ISubmodelElement submodelElement);
}