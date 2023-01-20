using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;

namespace AAS.ADT;

public interface IAasWriteConnector
{
    Task<Response<BasicDigitalTwin>> DoCreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twinData);

    Task<Response<BasicRelationship>> DoCreateOrReplaceRelationshipAsync(string sourceId,
        string relName, string targetId);

    Task<List<string>> DoCreateReferrableReferenceRelationships(List<string> refList, ISet<string> filteredTwins, List<string> result);
}