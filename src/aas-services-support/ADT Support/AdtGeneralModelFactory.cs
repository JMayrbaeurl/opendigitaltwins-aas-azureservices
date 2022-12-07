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
                languageStrings.Add(new LangString(langString.Key, langString.Value ));
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
            var referenceType = adtReference.Type == "ModelReference"
                ? ReferenceTypes.ModelReference
                : ReferenceTypes.GlobalReference;
            var reference= new Reference(referenceType,new List<Key>());
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
            return new Key((KeyTypes)Enum.Parse(typeof(KeyTypes), adtKey.Type), adtKey.Value);
        }

        public EmbeddedDataSpecification CreateEmbeddedDataSpecificationFromAdtDataSpecification(AdtDataSpecification twin)
        {
            var keys = new List<Key>()
                { new Key(KeyTypes.GlobalReference, twin.UnitIdValue) };
            var dataSpecification = new Reference(ReferenceTypes.GlobalReference,keys);
            

            // TODO implement DataSpecificationContent
            var dummyDataSpecificationContent = new DataSpecificationIec61360(new List<LangString>(){new LangString("de","dummy")}); var embeddedDataSpecification = new EmbeddedDataSpecification(dataSpecification,dummyDataSpecificationContent);

            return embeddedDataSpecification;
        }
    }
}
