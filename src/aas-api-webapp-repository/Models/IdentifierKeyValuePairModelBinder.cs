using System;
using System.Threading.Tasks;
using AAS.API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Aas.Api.Repository.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class IdentifierKeyValuePairModelBinder : IModelBinder
    {
        /// <summary>
        /// 
        /// </summary>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Specify a default argument name if none is set by ModelBinderAttribute
            var modelName = bindingContext.ModelName;
            if (String.IsNullOrEmpty(modelName))
            {
                modelName = "model";
            }

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            IdentifierKeyValuePair result = JsonConvert.DeserializeObject<IdentifierKeyValuePair>(valueProviderResult.FirstValue);
            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}
