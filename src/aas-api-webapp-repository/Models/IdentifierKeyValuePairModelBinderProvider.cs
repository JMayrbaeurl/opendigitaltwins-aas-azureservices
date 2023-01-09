using AAS.API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Aas.Api.Repository.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class IdentifierKeyValuePairModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(IdentifierKeyValuePair))
                return new IdentifierKeyValuePairModelBinder();

            return null;
        }
    }
}
