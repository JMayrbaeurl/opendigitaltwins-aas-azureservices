using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT.AutoMapper
{
    public class AdtReferenceProfile : AdtBaseProfile
    {
        public AdtReferenceProfile()
        {
            CreateMap<AdtReference, Reference>()
                .ForMember(d => d.Keys, o => o.MapFrom(s => mapAdtKeysToReferenceKeys(s)))
                .ForMember(d => d.ReferredSemanticId, o => o.Ignore())
                .ConstructUsing(d => new Reference(ReferenceTypes.GlobalReference,new List<Key>(),null));
        }

        private List<Key> mapAdtKeysToReferenceKeys(AdtReference adtReference)
        {
            var keys = new List<Key>();
            var adtKeys = GetAdtKeysAsList(adtReference);
            foreach (var adtKey in adtKeys)
            {
                if (adtKey != null)
                {
                    var keyType = new KeyTypes();
                    var keyTypeIsParseble = Enum.TryParse<KeyTypes>(adtKey.Type.ToString(), true, out keyType);
                    if (keyTypeIsParseble)
                    {
                        keys.Add(new Key(keyType, adtKey.Value));
                    }
                }
            }
            return keys;

        }

        private List<AdtKey> GetAdtKeysAsList(AdtReference adtReference)
        {
            return new List<AdtKey>()
            {
                adtReference.Key1,
                adtReference.Key2,
                adtReference.Key3,
                adtReference.Key4,
                adtReference.Key5,
                adtReference.Key6,
                adtReference.Key7,
                adtReference.Key8,
            };
        }
    }
}
