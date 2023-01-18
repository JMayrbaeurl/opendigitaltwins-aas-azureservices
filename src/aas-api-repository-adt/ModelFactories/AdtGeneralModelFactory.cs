using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt
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
            if (adtReference.Key1 == null)
            {
                return null;
            }
            var semanticId = ConvertAdtReferenceToGeneralReference(adtReference);
            semanticId.Keys = new List<Key>() { semanticId.Keys[0] };
            return semanticId;
        }

        private Reference ConvertAdtReferenceToGeneralReference(AdtReference adtReference)
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

        private Key ConvertAdtKeyToGeneralKey(AdtKey adtKey)
        {
            return new Key((KeyTypes)Enum.Parse(typeof(KeyTypes), adtKey.Type), adtKey.Value);
        }

        protected EmbeddedDataSpecification CreateEmbeddedDataSpecificationFromAdtDataSpecification(AdtDataSpecification twin)
        {
            var keys = new List<Key>()
                { new Key(KeyTypes.GlobalReference, twin.UnitIdValue) };
            var dataSpecification = new Reference(ReferenceTypes.GlobalReference, keys);

            
            var dummyDataSpecificationContent = new DataSpecificationIec61360(new List<LangString>() { new LangString("de", "dummy") }); 
            var embeddedDataSpecification = new EmbeddedDataSpecification(dataSpecification, dummyDataSpecificationContent);

            return embeddedDataSpecification;
        }

        
    }
}
