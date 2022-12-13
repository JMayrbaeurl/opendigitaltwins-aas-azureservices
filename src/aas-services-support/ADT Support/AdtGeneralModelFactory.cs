//using AAS.API.Models;
using AdtModels.AdtModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using System.Reflection;
using AAS.API.Services.ADT;
using AasCore.Aas3_0_RC02;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtGeneralModelFactory
    {

        public List<LangString> ConvertAdtLangStringToGeneraLangString(AdtLanguageString adtLangString)
        {
            var languageStrings = new List<LangString>();
            if (adtLangString.LangStrings == null)
            {
                return null;
            }
            foreach (var langString in adtLangString.LangStrings)
            {
                languageStrings.Add(new LangString(langString.Key, langString.Value));
            }

            return languageStrings;
        }

        public Reference GetSemanticId(AdtReference adtReference)
        {
            var semanticId = ConvertAdtReferenceToGeneralReference(adtReference);
            semanticId.Keys = new List<Key>() { semanticId.Keys[0] };
            return semanticId;
        }

        public Reference ConvertAdtReferenceToGeneralReference(AdtReference adtReference)
        {
            var referenceType = adtReference.Type == "ModelReference"
                ? ReferenceTypes.ModelReference
                : ReferenceTypes.GlobalReference;
            var reference = new Reference(referenceType, new List<Key>());
            reference.Keys = new List<Key>();

            for (int i = 0; i < 8; i++)
            {
                var adtKey = (AdtKey)adtReference.GetType().GetProperty($"Key{i + 1}")!.GetValue(adtReference);
                if (adtKey != null && adtKey.Type != null)
                {
                    reference.Keys.Add(ConvertAdtKeyToGeneralKey(adtKey));
                }
            }

            return reference;
        }

        public Key ConvertAdtKeyToGeneralKey(AdtKey adtKey)
        {
            return new Key((KeyTypes)Enum.Parse(typeof(KeyTypes), adtKey.Type), adtKey.Value);
        }

        public EmbeddedDataSpecification CreateEmbeddedDataSpecificationFromAdtDataSpecification(AdtDataSpecification twin)
        {
            var keys = new List<Key>()
                { new Key(KeyTypes.GlobalReference, twin.UnitIdValue) };
            var dataSpecification = new Reference(ReferenceTypes.GlobalReference, keys);

            
            var dummyDataSpecificationContent = new DataSpecificationIec61360(new List<LangString>() { new LangString("de", "dummy") }); 
            var embeddedDataSpecification = new EmbeddedDataSpecification(dataSpecification, dummyDataSpecificationContent);

            return embeddedDataSpecification;
        }

        protected DataSpecificationIec61360 GetIec61360DataSpecificationContent(AdtDataSpecificationIEC61360 adtIec61360)
        {
            var preferredName = ConvertAdtLangStringToGeneraLangString(adtIec61360.PreferredName);
            var definition = ConvertAdtLangStringToGeneraLangString( adtIec61360.Definition);
            var shortName = ConvertAdtLangStringToGeneraLangString(adtIec61360.ShortName);

            var iec61360 = new DataSpecificationIec61360(preferredName);
            iec61360.ShortName = shortName;
            iec61360.Definition = definition;
            iec61360.Value = adtIec61360.Value;
            iec61360.SourceOfDefinition = adtIec61360.SourceOfDefinition;
            iec61360.Symbol= adtIec61360.Symbol;
            iec61360.Unit = adtIec61360.Unit;
            iec61360.ValueFormat = adtIec61360.ValueFormat;
            return iec61360;
        }
    }
}
