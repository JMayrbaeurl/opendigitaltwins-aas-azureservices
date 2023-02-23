using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Aas.Api.Repository.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using AasCore.Aas3_0_RC02;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AAS.API.Models;
using AssetAdministrationShell = AasCore.Aas3_0_RC02.AssetAdministrationShell;
using Reference = AasCore.Aas3_0_RC02.Reference;

namespace AAS.API.Repository.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/shells")]
    public class AasRepositoryApi : Controller
    {

        private readonly AASRepository _repository;
        private readonly ILogger<AasRepositoryApi> _logger;

        /// <summary>
        /// 
        /// </summary>
        public AasRepositoryApi(AASRepository repository, ILogger<AasRepositoryApi> logger)
        {
            _repository = repository ??
                         throw new ArgumentNullException(); 
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
        public async Task<IActionResult> GetAllAssetAdministrationShells([FromQuery] string idShort)
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
                return StatusCode(200,JsonConvert.DeserializeObject(result));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return GeneralException();
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
                return StatusCode(201, shell);
            }
            catch (Jsonization.Exception e)
            {
                _logger.LogWarning(e, e.Message);
                return StatusCode(400, new Result()
                {
                    Success = false,
                    Messages = new List<Message>()
                    {
                        new Message()
                        {
                            MessageType = Message.MessageTypeEnum.ExceptionEnum,
                            Code = "400",
                            Text = e.Message,
                            Timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return GeneralException();
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
        public async Task<IActionResult> GetAssetAdministrationShellById([FromRoute][Required] string aasIdentifier)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(aasIdentifier);
                aasIdentifier = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                var aas = await _repository.GetAssetAdministrationShellWithId(aasIdentifier);

                // adds the "modelType" Attributes that are necessary for serialization
                var jsonObject = Jsonization.Serialize.ToJsonObject(aas);

                // removes unwanted Properties like "_value" from the jsonObject
                var result = jsonObject.ToJsonString();
                // The "Produces"-Attribute is set to "application/json" (in the Startup.cs)...
                // .. That means that the string "result" that's already in JsonFormat will be formatted again..
                // .. which leads to a wrong response containing escaped "-Character (\")
                return StatusCode(200, JsonConvert.DeserializeObject(result));
            }
            catch (AASRepositoryException e)
            {
                _logger.LogWarning(e,e.Message);
                return IdentifiableNotFoundException(e);

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return GeneralException();
            }

        }

        /// <summary>
        /// Updates an existing Asset Administration Shell
        /// </summary>
        /// <param name="body">Asset Administration Shell object</param>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <response code="204">Asset Administration Shell updated successfully</response>
        [HttpPut]
        [Route("{aasIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("PutAssetAdministrationShellById")]
        public async Task<IActionResult> PutAssetAdministrationShellById([FromBody] JObject body,
            [FromRoute] [Required] string aasIdentifier)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(aasIdentifier);
            aasIdentifier = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            try
            {
                var bodyParsed = JsonNode.Parse(body.ToString());
                var assetAdministrationShell = Jsonization.Deserialize.AssetAdministrationShellFrom(bodyParsed);

                await _repository.UpdateExistingAssetAdministrationShellWithId(aasIdentifier, assetAdministrationShell);
                return StatusCode(204);
            }
            catch (Jsonization.Exception e)
            {
                _logger.LogWarning(e, e.Message);
                return StatusCode(400, new Result()
                {
                    Success = false,
                    Messages = new List<Message>()
                    {
                        new Message()
                        {
                            MessageType = Message.MessageTypeEnum.ExceptionEnum,
                            Code = "400",
                            Text = e.Message,
                            Timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                });
            }
            catch (AASRepositoryException e)
            {
                _logger.LogWarning(e, e.Message);
                return IdentifiableNotFoundException(e);

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return GeneralException();
            }
        }

        /// <summary>
        /// Deletes an Asset Administration Shell
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <response code="204">Asset Administration Shell deleted successfully</response>
        [HttpDelete]
        [Route("{aasIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("DeleteAssetAdministrationShellById")]
        public async Task<IActionResult> DeleteAssetAdministrationShellById([FromRoute][Required] string aasIdentifier)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(aasIdentifier);
            aasIdentifier = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            try
            {
                await _repository.DeleteAssetAdministrationShellWithId(aasIdentifier);
                return StatusCode(200);
            }
            catch (AASRepositoryException e)
            {
                _logger.LogWarning(e, e.Message);
                return IdentifiableNotFoundException(e);

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return GeneralException();
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
                var base64EncodedBytes = System.Convert.FromBase64String(aasIdentifier);
                aasIdentifier = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                await _repository.CreateSubmodelReference(aasIdentifier, body);
                return Ok();
            }
            catch (AASRepositoryException e)
            {
                _logger.LogWarning(e, e.Message);
                return IdentifiableNotFoundException(e);

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return GeneralException();
            }

        }

        /// <summary>
        /// Deletes the submodel reference from the Asset Administration Shell
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <response code="204">Submodel reference deleted successfully</response>
        [HttpDelete]
        [Route("{aasIdentifier}/aas/submodels/{submodelIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("DeleteSubmodelReferenceById")]
        public async Task<IActionResult> DeleteSubmodelReferenceById([FromRoute] [Required] string aasIdentifier,
            [FromRoute] [Required] string submodelIdentifier)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(aasIdentifier);
                aasIdentifier = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                await _repository.DeleteSubmodelReference(aasIdentifier, submodelIdentifier);
                return Ok();
            }
            catch (AASRepositoryException e)
            {
                _logger.LogWarning(e, e.Message);
                return IdentifiableNotFoundException(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return GeneralException();
            }
        }

        private IActionResult GeneralException()
        {
            return StatusCode(500, new Result()
            {
                Success = false,
                Messages = new List<Message>()
                {
                    new Message()
                    {
                        MessageType = Message.MessageTypeEnum.ExceptionEnum,
                        Code = "500", Text = "Exception in AAS repository", Timestamp = DateTime.UtcNow.ToString("o")
                    }
                }
            });
        }

        private IActionResult IdentifiableNotFoundException(Exception e)
        {
            return StatusCode(404, new Result()
            {
                Success = false,
                Messages = new List<Message>()
                {
                    new Message()
                    {
                        MessageType = Message.MessageTypeEnum.ExceptionEnum,
                        Code = "404", Text = e.Message, Timestamp = DateTime.UtcNow.ToString("o")
                    }
                }
            });
        }

    }
}