using AAS.API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AAS.API.Registry.Models
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
            var jsonString = bindingContext.ActionContext.HttpContext.Request.Query["assetIds"];
            IdentifierKeyValuePair result = JsonConvert.DeserializeObject<IdentifierKeyValuePair>(jsonString);

            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }
}
