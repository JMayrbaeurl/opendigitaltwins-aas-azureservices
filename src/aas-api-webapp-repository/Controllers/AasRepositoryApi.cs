using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AAS.API.Registry.Attributes;

using Microsoft.AspNetCore.Authorization;
using AAS.API.Models;
using Microsoft.Extensions.Configuration;
using AAS.API.Repository;

namespace AAS.API.Registry.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/shells")]
    public class AasRepositoryApi : Controller
    {

        private IConfiguration _configuration;

        private AASRepository repository;


        /// <summary>
        /// 
        /// </summary>
        public AasRepositoryApi(IConfiguration config) : base()
        {
            _configuration = config ??
                             throw new ArgumentNullException(nameof(config));

            repository = new AASRepositoryFactory().CreateAASRepositoryForADT(config["ADT_SERVICE_URL"]) ??
                         throw new ArgumentNullException(); ;
        }


        /// <summary>
        /// Returns all Asset Administration Shells
        /// </summary>
        /// <param name="assetIds">The key-value-pair of an Asset identifier</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <response code="200">Requested Asset Administration Shells</response>
        [HttpGet]
        [ValidateModelState]
        [SwaggerOperation("GetAllAssetAdministrationShells")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<AssetAdministrationShell>), description: "Requested Asset Administration Shells")]
        public async Task<ActionResult<List<AssetAdministrationShell>>> GetAllAssetAdministrationShells([FromQuery(Name = "assetIds")] List<IdentifierKeyValuePair> assetIds, [FromQuery] string idShort)
        {
            //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...

            var aas = await repository.GetAllAdministrationShells();
            return Ok(aas);

            //if (idShort != null && idShort.Length > 0)
            //{
            //    return new ObjectResult(repository.GetAllAssetAdministrationShellsByIdShort(idShort).GetAwaiter().GetResult());
            //}
            //else
            //{
            //    if (assetIds != null && assetIds.Count > 0)
            //        return new ObjectResult(repository.GetAllAssetAdministrationShellsByAssetId(assetIds).GetAwaiter().GetResult());
            //    else
            //    {
            //        var aas = await repository.GetAllAdministrationShells();
            //        return Ok(aas); //new ObjectResult(.GetAwaiter().GetResult());
            //    }
            // }
        }


    }
}