using System;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Newtonsoft.Json.Linq;

namespace AAS.API.Repository
{
    public class JObjectToPropertyMapper  : ObjectMapperStrategy<Property>
    {
        private readonly IMapper _autoMapper;
        public JObjectToPropertyMapper(IMapper autoMapper)
        {
            _autoMapper = autoMapper ??
                          throw new ArgumentNullException(nameof(autoMapper));
        }

        public Property map(JObject obj)
        {
            var valueType = obj.GetValue("valueType").ToString() == null
                ? throw new ArgumentNullException()
                : _autoMapper.Map<DataTypeDefXsd>(obj.GetValue("valueType").ToString());
            var property = new Property(valueType);
            property.Value = obj.GetValue("value").ToString();
            property.ValueId = obj.GetValue("valueId").ToObject<Reference>();
            property.AddSubmodelElementValues(obj);
            return property;
        }
    }


}
