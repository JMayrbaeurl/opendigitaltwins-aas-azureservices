using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;

namespace AAS_Services_Support.ADT_Support;

public interface IAdtSubmodelModelFactory
{
    Task<Submodel> GetSubmodel(AdtSubmodelAndSmcInformation<AdtSubmodel> information);
}