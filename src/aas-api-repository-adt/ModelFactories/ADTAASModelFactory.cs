using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.API.Repository.Adt
{
    public class ADTAASModelFactory : AdtGeneralModelFactory
    {
        private readonly IAdtAasConnector _adtAasConnector;
        private readonly IMapper _mapper;

        public ADTAASModelFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        public AssetAdministrationShell GetAas(AdtAssetAdministrationShellInformation information)
        {
            var aas = _mapper.Map<AssetAdministrationShell>(information.RootElement);

            aas.Submodels = new List<Reference>();
            aas.EmbeddedDataSpecifications = new List<EmbeddedDataSpecification>();
            if (information.AssetInformation != null)
            {
                aas.AssetInformation = CreateAssetInformationFromAdtAssetInformation(information.AssetInformation);
            }

            foreach (var adtSubmodel in information.Submodels)
            {
                aas.Submodels.Add(GetSubmodelReferenceFromAdtSubmodel(adtSubmodel));
            }

            if (information.DerivedFrom != null)
            {
                aas.DerivedFrom = CreateDerivedFromFromAdtAas(information.DerivedFrom);
            }
            
            return aas;
        }

        private AssetInformation CreateAssetInformationFromAdtAssetInformation(AdtAssetInformation adtAssetInformation)
        {
            var assetKind = adtAssetInformation.AssetKind.AssetKind.ToLower() == "instance"
                ? AssetKind.Instance
                : AssetKind.Type;
            var assetInformation = new AssetInformation(assetKind);

            var key = new Key(KeyTypes.GlobalReference, adtAssetInformation.GlobalAssetId);
            var globalAssetId = new Reference(ReferenceTypes.GlobalReference, new List<Key>() { key });
            assetInformation.GlobalAssetId = globalAssetId;

            assetInformation.SpecificAssetIds = new List<SpecificAssetId>();

            // TODO implement specific AssetId
            assetInformation.SpecificAssetIds = null;
            
            return assetInformation;

        }
        private static Reference GetSubmodelReferenceFromAdtSubmodel(AdtSubmodel submodel)
        {
            var reference = new Reference(ReferenceTypes.ModelReference, new List<Key>() { new Key(KeyTypes.Submodel, submodel.Id) });
            return reference;
        }

        private static Reference CreateDerivedFromFromAdtAas(AdtAas aas)
        {
            var keys = new List<Key>()
                { new Key(KeyTypes.AssetAdministrationShell, aas.Id) };

            return new Reference(ReferenceTypes.ModelReference, keys);
        }


    }
}
