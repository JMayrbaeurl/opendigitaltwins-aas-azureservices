using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.ADT;

public interface IAasDeleteAdt
{
    public Task DeleteTwin(string twinId);
    public Task DeleteRelationship(string sourceTwinId, string targetTwinId, string relationshipName);

}