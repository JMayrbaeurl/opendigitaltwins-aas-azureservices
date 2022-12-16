using System.Collections.Generic;
using AasCore.Aas3_0_RC02;

namespace AAS_Services_Support.ADT_Support;

public interface IAdtDefinitionsAndSemanticsModelFactory
{
    List<Reference> GetSupplementalSemanticIdsForTwin(string twinId);
    List<EmbeddedDataSpecification> GetEmbeddedDataSpecificationsForTwin(string dtId);
    public void Configure(DefinitionsAndSemantics definitionsAndSemantics);
}