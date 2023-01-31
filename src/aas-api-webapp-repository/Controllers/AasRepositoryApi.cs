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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

namespace AAS.API.Repository.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/shells")]
    public class AasRepositoryApi : Controller
    {

        private readonly AASRepository _repository;
        private readonly ILogger<AasRepositoryApi> _logger;

        /// <summary>
        /// 
        /// </summary>
        public AasRepositoryApi(IConfiguration config, IAASRepositoryFactory aasRepositoryFactory, ILogger<AasRepositoryApi> logger)
        {
            _repository = aasRepositoryFactory.CreateAASRepositoryForADT(config["ADT_SERVICE_URL"]) ??
                         throw new ArgumentNullException(); ;
            _logger = logger ??
                      throw new ArgumentNullException(nameof(logger));

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
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

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
        public async Task<IActionResult> PostAssetAdministrationShell([FromBody] JObject body)
        {
            try
            {
                var bodyParsed = JsonNode.Parse(body.ToString());
                var shell = Jsonization.Deserialize.AssetAdministrationShellFrom(bodyParsed);
                await _repository.CreateAssetAdministrationShell(shell);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

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
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

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
        public async Task<IActionResult> PostSubmodelReference([FromBody] Reference body,
            [FromRoute][Required] string aasIdentifier)
        {
            try
            {
                aasIdentifier = System.Web.HttpUtility.UrlDecode(aasIdentifier);
                await _repository.CreateSubmodelReference(aasIdentifier, body);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }

    }
}