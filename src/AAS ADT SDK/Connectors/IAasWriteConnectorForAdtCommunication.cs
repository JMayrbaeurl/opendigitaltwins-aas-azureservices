using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;

namespace AAS.ADT;

public interface IAasWriteConnector
{
    Task DoCreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twinData);

    Task DoCreateOrReplaceRelationshipAsync(string sourceId,
        string relName, string targetId);
}