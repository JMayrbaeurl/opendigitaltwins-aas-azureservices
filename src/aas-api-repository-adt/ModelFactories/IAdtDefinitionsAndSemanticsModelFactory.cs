﻿using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt;

public interface IAdtDefinitionsAndSemanticsModelFactory
{
    public Reference? GetSemanticId(AdtReference adtReference);
    public Reference? GetSemanticIdForTwin(string twinId, DefinitionsAndSemantics definitionsAndSemantics);
    List<Reference>? GetSupplementalSemanticIdsForTwin(string twinId, DefinitionsAndSemantics definitionsAndSemantics);
    List<EmbeddedDataSpecification>? GetEmbeddedDataSpecificationsForTwin(string dtId, DefinitionsAndSemantics definitionsAndSemantics);
}