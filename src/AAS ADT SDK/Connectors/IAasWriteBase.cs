using System.Collections.Generic;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT;

public interface IAasWriteBase
{
    Task AddReference(string sourceTwinId, Reference reference, string relationshipName);

    Task AddHasDataSpecification(string sourceTwinId,
        List<EmbeddedDataSpecification> embeddedDataSpecifications);

    Task AddQualifiableRelations(string sourceTwinId, List<Qualifier> qualifiers);
}