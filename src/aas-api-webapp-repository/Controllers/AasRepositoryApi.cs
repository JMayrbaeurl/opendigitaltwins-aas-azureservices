using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AAS.API.Repository;
using Aas.Api.Repository.Attributes;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using AasCore.Aas3_0_RC02;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AAS.API.Repository.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/shells")]
    public class AasRepositoryApi : Controller
    {

        private readonly AASRepository _repository;

        /// <summary>
        /// 
        /// </summary>
        public AasRepositoryApi(IConfiguration config, IAASRepositoryFactory aasRepositoryFactory)
        {
            _repository = aasRepositoryFactory.CreateAASRepositoryForADT(config["ADT_SERVICE_URL"]) ??
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
        public async Task<ActionResult<List<AssetAdministrationShell>>> GetAllAssetAdministrationShells([FromQuery] string idShort)
        {
            var assetAdministrationShells = await _repository.GetAllAssetAdministrationShells();
            var result = "[";
            for (int i = 0; i < assetAdministrationShells.Count; i++)
            {
                var temp = Jsonization.Serialize.ToJsonObject(assetAdministrationShells[i]);
                if (i > 0)
                {
                    result += ",";
                }

                result += temp.ToJsonString();
            }

            result += "]";

            // The "Produces"-Attribute is set to "application/json" (in the Startup.cs)...
            // .. That means that the string "result" that's already in JsonFormat will be formatted again..
            // .. which leads to a wrong response containing escaped "-Character (\")
            return Ok(JsonConvert.DeserializeObject(result));
        }

        /// <summary>
        /// Creates a new Asset Administration Shell
        /// </summary>
        /// <param name="body">Asset Administration Shell object</param>
        /// <response code="201">Asset Administration Shell created successfully</response>
        [HttpPost]
        [ValidateModelState]
        [SwaggerOperation("PostAssetAdministrationShell")]
        [SwaggerResponse(statusCode: 201, type: typeof(AssetAdministrationShell),
            description: "Asset Administration Shell created successfully")]
        public virtual IActionResult PostAssetAdministrationShell([FromBody] JObject body)
        {
            var bodyParsed = JsonNode.Parse(body.ToString());
            var shell  = Jsonization.Deserialize.AssetAdministrationShellFrom(bodyParsed);
            _repository.CreateAssetAdministrationShell(shell);
            return Ok();
        }


        /// <summary>
        /// Returns a specific Asset Administration Shell
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <response code="200">Requested Asset Administration Shell</response>
        [HttpGet]
        [Route("{aasIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("GetAssetAdministrationShellById")]
        [SwaggerResponse(statusCode: 200, type: typeof(AssetAdministrationShell), description: "Requested Asset Administration Shell")]
        public async Task<ActionResult<AssetAdministrationShell>> GetAssetAdministrationShellById([FromRoute][Required] string aasIdentifier)
        {
            aasIdentifier = System.Web.HttpUtility.UrlDecode(aasIdentifier);
            var aas = await _repository.GetAssetAdministrationShellWithId(aasIdentifier);
            
            // adds the "modelType" Attributes that are necessary for serialization
            var jsonObject = Jsonization.Serialize.ToJsonObject(aas);

            // removes unwanted Properties like "_value" from the jsonObject
            var result = jsonObject.ToJsonString();
            // The "Produces"-Attribute is set to "application/json" (in the Startup.cs)...
            // .. That means that the string "result" that's already in JsonFormat will be formatted again..
            // .. which leads to a wrong response containing escaped "-Character (\")
            return Ok(JsonConvert.DeserializeObject(result));
        }

        /// <summary>
        /// Creates a submodel reference at the Asset Administration Shell
        /// </summary>
        /// <param name="body">Reference to the Submodel</param>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <response code="201">Submodel reference created successfully</response>
        [HttpPost]
        [Route("{aasIdentifier}/aas/submodels")]
        [ValidateModelState]
        [SwaggerOperation("PostSubmodelReference")]
        [SwaggerResponse(statusCode: 201, type: typeof(Reference),
            description: "Submodel reference created successfully")]
        public virtual IActionResult PostSubmodelReference([FromBody] Reference body,
            [FromRoute] [Required] string aasIdentifier)
        {

            aasIdentifier = System.Web.HttpUtility.UrlDecode(aasIdentifier);
            _repository.CreateSubmodelReference(aasIdentifier, body);
            return Ok();
        }

    }
}