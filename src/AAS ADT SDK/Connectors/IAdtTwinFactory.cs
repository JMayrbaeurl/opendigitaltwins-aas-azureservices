using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;

namespace AAS.ADT;

public interface IAdtTwinFactory
{
    public BasicDigitalTwin CreateTwin(ISubmodelElement submodelElement, string modelName);
}