using Azure;
using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AdtModels.AdtModels;
using System.Threading.Tasks;
using AAS_Services_Support.ADT_Support;
using AAS.API.Interfaces;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Services.ADT
{
    public class ADTAASModelFactory : AdtGeneralModelFactory
    {
        private readonly IAdtInteractions _adtInteractions;

        public ADTAASModelFactory(IAdtInteractions adtInteractions) : base()
        {
            _adtInteractions = adtInteractions;
        }

        public AssetAdministrationShell GetAasWithId(string aasId)
        {
            var adtAas = _adtInteractions.GetAdtAasForAasWithId(aasId);
            var aas = ConvertAdtAasToAas(adtAas);
            aas.Submodels = new List<Reference>();
            aas.EmbeddedDataSpecifications = new List<EmbeddedDataSpecification>();


            var aasInformations = _adtInteractions.GetAllInformationForAasWithId(aasId);
            foreach (var aasInformation in aasInformations)
            {
                if (aasInformation.RelationshipName == "assetInformation")
                {
                    var adtAssetInformation = JsonSerializer.Deserialize<AdtAssetInformation>(aasInformation.TwinJsonObject.ToString());
                    var assetInformation = CreateAssetInformationFromAdtAssetInformation(adtAssetInformation);
                    aas.AssetInformation = assetInformation;
                }
                else if (aasInformation.RelationshipName == "submodel")
                {
                    var submodel =
                        JsonSerializer.Deserialize<AdtSubmodel>(aasInformation.TwinJsonObject.ToString());
                    aas.Submodels.Add(GetSubmodelReferenceFromAdtSubmodel(submodel));
                }
                else if (aasInformation.RelationshipName == "derivedFrom")
                {
                    var derivedFrom =
                        JsonSerializer.Deserialize<AdtAas>(aasInformation.TwinJsonObject.ToString());
                    aas.DerivedFrom = CreateDerivedFromFromAdtAas(derivedFrom);

                }
                else if (aasInformation.RelationshipName == "dataSpecification")
                {
                    var dataSpecification = JsonSerializer.Deserialize<AdtDataSpecification>(aasInformation.TwinJsonObject.ToString());
                    aas.EmbeddedDataSpecifications.Add(
                        CreateEmbeddedDataSpecificationFromAdtDataSpecification(dataSpecification));
                }
            }

            return aas;
        }

        private AssetAdministrationShell ConvertAdtAasToAas(AdtAas adtAas)
        {
            // TODO Add Asset Information
            var assetInformation = new AssetInformation(AssetKind.Instance);
            var aas = new AssetAdministrationShell(adtAas.Id,assetInformation);
            aas.IdShort = adtAas.IdShort;
            aas.DisplayName = ConvertAdtLangStringToGeneraLangString(adtAas.DisplayName);
            aas.Description = ConvertAdtLangStringToGeneraLangString(adtAas.Description);
            aas.Category = adtAas.Category;
            aas.Administration = convertAdtAdministrationToAdministrativeInformation(adtAas.Administration);
            aas.Checksum = adtAas.Checksum;
            return aas;
        }

        private AdministrativeInformation convertAdtAdministrationToAdministrativeInformation(
            AdtAdministration adtAdministration)
        {
            var administration = new AdministrativeInformation();
            administration.Revision = adtAdministration.Revision;
            administration.Version = adtAdministration.Version;
            return administration;
        }

        private AssetInformation CreateAssetInformationFromAdtAssetInformation(AdtAssetInformation adtAssetInformation)
        {
            var assetKind = adtAssetInformation.AssetKind.AssetKind == "Instance"
                ? AssetKind.Instance
                : AssetKind.Type;
            var assetInformation = new AssetInformation(assetKind);
            
            var key = new Key(KeyTypes.GlobalReference,adtAssetInformation.GlobalAssetId );
            var globalAssetId = new Reference(ReferenceTypes.GlobalReference,new List<Key>(){key});
            assetInformation.GlobalAssetId = globalAssetId;

            assetInformation.SpecificAssetIds = new List<SpecificAssetId>();
            
            // TODO implement specific AssetId
            var specificRawAssetIds = adtAssetInformation.SpecificAssetId.Split(',');
            foreach (var specificRawAssetId in specificRawAssetIds)
            {
                var temp = specificRawAssetId.Replace(" ","").Split(")");
                //TODO Not just create dummy reference
                var specificAssetId = new SpecificAssetId(temp[0].Replace("(",""), temp[1],
                    new Reference(ReferenceTypes.GlobalReference,new List<Key>()));
                assetInformation.SpecificAssetIds.Add(specificAssetId);
            }
            return assetInformation;

        }
        private static Reference GetSubmodelReferenceFromAdtSubmodel(AdtSubmodel submodel)
        {
            var reference = new Reference(ReferenceTypes.ModelReference,new List<Key>(){ new Key(KeyTypes.Submodel,submodel.Id) });
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
