using AAS.API.Models;
using AdtModels.AdtModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using System.Reflection;
using AAS.API.Services.ADT;

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
                languageStrings.Add(new LangString() { Language = langString.Key, Text = langString.Value });
            }

            return languageStrings;
        }

        //public Reference GetSemanticId(string parentTwinId)
        //{
        //    AdtReference adtReference;
        //    try
        //    {
        //        adtReference = _adtInteractions.GetSemanticId(parentTwinId);

        //    }
        //    catch (NoSemanticIdFound e)
        //    {
        //        return null;
        //    }

        //    return ConvertAdtReferenceToGeneralReference(adtReference);

        //}

        public Reference ConvertAdtReferenceToGeneralReference(AdtReference adtReference)
        {
            var reference= new Reference();
            reference.Type = adtReference.Type == "ModelReference"
                ? ReferenceTypes.ModelReferenceEnum
                : ReferenceTypes.GlobalReferenceEnum;
            reference.Keys = new List<Key>();
            
            for (int i = 0; i < 8; i++)
            {
                var adtKey = (AdtKey)adtReference.GetType().GetProperty($"Key{i+1}")!.GetValue(adtReference);
                if (adtKey != null && adtKey.Type != null)
                {
                    reference.Keys.Add(ConvertAdtKeyToGeneralKey(adtKey));
                }
            }

            return reference;
        }

        public Key ConvertAdtKeyToGeneralKey(AdtKey adtKey)
        {
            var keyTypeString = $"{adtKey.Type}Enum";
            var key = new Key();
            key.Type = (KeyTypes)Enum.Parse(typeof(KeyTypes), keyTypeString);
            key.Value= adtKey.Value;
            return key;
        }

        public EmbeddedDataSpecification CreateEmbeddedDataSpecificationFromAdtDataSpecification(AdtDataSpecification twin)
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
