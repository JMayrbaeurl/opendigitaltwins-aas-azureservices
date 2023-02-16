using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT;

public interface IAasWriteSubmodelElements
{
    Task<string> CreateSubmodelElement(ISubmodelElement submodelElement);
}