using System.Collections.Generic;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;

namespace AAS.ADT;

public interface IAdtTwinFactory
{
    public BasicDigitalTwin GetTwin(ISubmodelElement submodelElement);
    public BasicDigitalTwin GetTwin(Reference reference);
    public BasicDigitalTwin GetTwin(IDataSpecificationContent content);
    public BasicDigitalTwin GetTwin(Qualifier qualifiers);
}