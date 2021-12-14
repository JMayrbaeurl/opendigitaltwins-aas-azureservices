using AAS.API.Models;
using Microsoft.AspNetCore.Http;
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
            if (jsonString.Count <= 1)
            {
                IdentifierKeyValuePair result = JsonConvert.DeserializeObject<IdentifierKeyValuePair>(jsonString);
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else
            {
                List<IdentifierKeyValuePair> result = new List<IdentifierKeyValuePair>();
                foreach (var item in jsonString)
                {
                    result.Add(JsonConvert.DeserializeObject<IdentifierKeyValuePair>(item));
                }
                bindingContext.Result = ModelBindingResult.Success(result);
            }

            return Task.CompletedTask;
        }
    }
}
