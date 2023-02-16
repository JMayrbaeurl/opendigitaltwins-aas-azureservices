using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;

namespace AAS.ADT;

public interface IAasWriteConnector
{
    Task<string> DoCreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twinData);

    Task<string> DoCreateOrReplaceRelationshipAsync(string sourceId,
        string relName, string targetId);
}