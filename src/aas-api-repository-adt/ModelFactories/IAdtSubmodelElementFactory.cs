using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt;

public interface IAdtSubmodelElementFactory
{
    List<ISubmodelElement> GetSubmodelElements(
        AdtSubmodelElements adtSubmodelElements, DefinitionsAndSemantics definitionsAndSemantics);
}