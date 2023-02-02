using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelModelFactory : IAdtSubmodelModelFactory
    {
        private readonly IAdtSubmodelElementFactory _adtSubmodelElementFactory;
        private readonly IAdtDefinitionsAndSemanticsModelFactory _definitionsAndSemanticsFactory;
        private readonly IMapper _mapper;

        public AdtSubmodelModelFactory(IAdtDefinitionsAndSemanticsModelFactory definitionsAndSemanticsFactory,
            IAdtSubmodelElementFactory adtSubmodelElementFactory, IMapper mapper)
        {
            _adtSubmodelElementFactory = adtSubmodelElementFactory;
            _mapper = mapper??
                      throw new ArgumentNullException(nameof(mapper));
            _definitionsAndSemanticsFactory = definitionsAndSemanticsFactory;
        }


        public Submodel GetSubmodel(AdtSubmodelAndSmcInformation<AdtSubmodel> information)
        {
            var submodelTwinId = information.RootElement.dtId;
            
            var submodel = _mapper.Map<Submodel>(information.RootElement);
            
            submodel.SemanticId =
                _definitionsAndSemanticsFactory.GetSemanticIdForTwin(submodelTwinId,
                    information.DefinitionsAndSemantics);

            submodel.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                .GetEmbeddedDataSpecificationsForTwin(submodelTwinId,
                    information.DefinitionsAndSemantics);

            submodel.SupplementalSemanticIds = _definitionsAndSemanticsFactory.GetSupplementalSemanticIdsForTwin(
                submodelTwinId, information.DefinitionsAndSemantics);
            
            submodel.SubmodelElements = _adtSubmodelElementFactory.GetSubmodelElements(
                information.AdtSubmodelElements,information.DefinitionsAndSemantics);

            return submodel;
        }
    }
}
