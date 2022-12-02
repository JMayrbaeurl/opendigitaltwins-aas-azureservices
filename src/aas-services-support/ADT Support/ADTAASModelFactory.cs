using AAS.API.Models;
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

namespace AAS.API.Services.ADT
{
    public class ADTAASModelFactory
    {
        private readonly AdtInteractions _adtInteractions;

        public ADTAASModelFactory(DigitalTwinsClient client)
        {
            _adtInteractions = new AdtInteractions(client);
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
            var aas = new AssetAdministrationShell();
            aas.Id = adtAas.Id;
            aas.IdShort = adtAas.IdShort;
            aas.DisplayName = ConvertAdtLangStringToGeneraLangString(adtAas.DisplayName);
            aas.Description = ConvertAdtLangStringToGeneraLangString(adtAas.Description);
            aas.Category = adtAas.Category;
            aas.Administration = convertAdtAdministrationToAdministrativeInformation(adtAas.Administration);
            aas.Checksum = adtAas.Checksum;
            return aas;
        }

        private List<LangString> ConvertAdtLangStringToGeneraLangString(AdtLanguageString adtLangString)
        {
            var languageStrings = new List<LangString>();
            if (adtLangString.LangStrings == null)
            {
                return null;
            }
            foreach (var langString in adtLangString.LangStrings)
            {
                languageStrings.Add(new LangString() { Language = langString.Key, Text = langString.Value });
            }

            return languageStrings;
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
            var assetInformation = new AssetInformation();
            assetInformation.AssetKind = adtAssetInformation.AssetKind.AssetKind == "Instance"
                ? AssetKind.InstanceEnum
                : AssetKind.TypeEnum;
            var globalAssetId = new Reference();
            globalAssetId.Keys = new List<Key>();
            var key = new Key();
            key.Value = adtAssetInformation.GlobalAssetId;
            key.Type = KeyTypes.GlobalReferenceEnum;
            globalAssetId.Keys.Add(key);
            assetInformation.GlobalAssetId = globalAssetId;

            assetInformation.SpecificAssetIds = new List<SpecificAssetId>();
            var specificAssetId = new SpecificAssetId();
            specificAssetId.Value = adtAssetInformation.SpecificAssetId;
            assetInformation.SpecificAssetIds.Add(specificAssetId);

            return assetInformation;

        }
        private static Reference GetSubmodelReferenceFromAdtSubmodel(AdtSubmodel submodel)
        {
            var reference = new Reference();
            reference.Keys = new List<Key>();
            var key = new Key();
            key.Value = submodel.Id;
            key.Type = KeyTypes.SubmodelEnum;
            reference.Keys.Add(key);

            reference.Type = ReferenceTypes.ModelReferenceEnum;
            return reference;
        }

        private static Reference CreateDerivedFromFromAdtAas(AdtAas aas)
        {
            var derivedFrom = new Reference();

            derivedFrom.Keys = new List<Key>()
                { new() { Type = KeyTypes.AssetAdministrationShellEnum, Value = aas.Id } };

            if (derivedFrom.Keys == null)
            {
                return null;
            }

            derivedFrom.Type = ReferenceTypes.ModelReferenceEnum;
            return derivedFrom;
        }

        private EmbeddedDataSpecification CreateEmbeddedDataSpecificationFromAdtDataSpecification(AdtDataSpecification twin)
        {
            var dataSpecification = new Reference();
            dataSpecification.Type = ReferenceTypes.GlobalReferenceEnum;
            dataSpecification.Keys = new List<Key>()
            { new() { Type = KeyTypes.GlobalReferenceEnum, Value = twin.UnitIdValue } };

            var embeddedDataSpecification = new EmbeddedDataSpecification();
            embeddedDataSpecification.DataSpecification = dataSpecification;

            // TODO implement DataSpecificationContent
            embeddedDataSpecification.DataSpecificationContent = new DataSpecificationContent();
            return embeddedDataSpecification;
        }
    }
}
