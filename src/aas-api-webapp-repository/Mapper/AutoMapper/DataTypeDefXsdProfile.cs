using System;
using System.Diagnostics;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace Aas.Api.Repository.Mapper.AutoMapper
{

    public class DataTypeDefXsdProfile :  Profile
    {
        public DataTypeDefXsdProfile()
        {
            CreateMap<string, DataTypeDefXsd>()
                .ConvertUsing(new StringToDataTypeDefXsdConverter());
        }
    }

    public class StringToDataTypeDefXsdConverter : ITypeConverter<string, DataTypeDefXsd>
    {
        public DataTypeDefXsd Convert(string source, DataTypeDefXsd destination, ResolutionContext context)
        {
            var asdf = source.StartsWith("xs:", StringComparison.InvariantCultureIgnoreCase) 
                ? source[3..source.Length] 
                : source;
            var result = Enum.Parse<DataTypeDefXsd>(asdf, true);
            return result;
        }
    }
}
