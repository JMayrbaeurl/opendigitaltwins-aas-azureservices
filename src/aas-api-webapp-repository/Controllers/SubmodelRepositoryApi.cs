using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Aas.Api.Repository.Attributes;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using AasCore.Aas3_0_RC02;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AAS.API.Repository.Controllers
{
    [Route("api/v1/submodels")]
    [ApiController]
    public class SubmodelRepositoryApi : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISubmodelRepository _repository;
        private readonly ILogger<SubmodelRepositoryApi> _logger;
        private readonly IMapper _mapper;


        public SubmodelRepositoryApi(IConfiguration config, ISubmodelRepository repository, IMapper mapper, ILogger<SubmodelRepositoryApi> logger)
        {
            _configuration = config ??
                             throw new ArgumentNullException(nameof(config));

            _repository = repository ??
                          throw new ArgumentNullException(nameof(repository));
            _logger = logger ??
                      throw new ArgumentNullException(nameof(logger));
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all Submodels
        /// </summary>
        /// <param name="semanticId">The value of the semantic id reference (BASE64-URL-encoded)</param>
        /// <param name="idShort">The Submodel’s idShort</param>
        /// <response code="200">Requested Submodels</response>
        [HttpGet]
        [ValidateModelState]
        [SwaggerOperation("GetAllSubmodels")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submodel>), description: "Requested Submodels")]
        public async Task<ActionResult<List<Submodel>>>GetAllSubmodels([FromQuery] string semanticId, [FromQuery] string idShort)
        {
            var submodels = await _repository.GetAllSubmodels();
            var result = "[";
            for (int i = 0; i < submodels.Count; i++)
            {
                var temp = Jsonization.Serialize.ToJsonObject(submodels[i]);
                if (i>0)
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
        /// Creates a new Submodel
        /// </summary>
        /// <param name="body">Submodel object</param>
        /// <response code="201">Submodel created successfully</response>
        [HttpPost]
        [ValidateModelState]
        [SwaggerOperation("PostSubmodel")]
        [SwaggerResponse(statusCode: 201, type: typeof(Submodel), description: "Submodel created successfully")]
        public async Task<IActionResult> PostSubmodel([FromBody] JObject body)
        {
            var bodyParsed = JsonNode.Parse(body.ToString());
            var submodel = Jsonization.Deserialize.SubmodelFrom(bodyParsed);

            await _repository.CreateSubmodel(submodel);
            return Ok();
        }

        /// <summary>
        /// Returns the Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="content">Determines the request or response kind of the resource</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <response code="200">Requested Submodel</response>
        [HttpGet]
        [Route("{submodelIdentifier}/submodel")]
        [ValidateModelState]
        [SwaggerOperation("GetSubmodelSubmodelRepo")]
        [SwaggerResponse(statusCode: 200, type: typeof(Submodel), description: "Requested Submodel")]
        public async Task<IActionResult> GetSubmodelSubmodelRepo([FromRoute][Required] string submodelIdentifier, [FromQuery] string level, [FromQuery] string content, [FromQuery] string extent)
        {
            submodelIdentifier = System.Web.HttpUtility.UrlDecode(submodelIdentifier);
            var submodel = await _repository.GetSubmodelWithId(submodelIdentifier);
            
            // adds the "modelType" Attributes that are necessary for serialization
            var jsonObject = Jsonization.Serialize.ToJsonObject(submodel);

            // removes unwanted Properties like "_value" from the jsonObject
            var result = jsonObject.ToJsonString();
            // The "Produces"-Attribute is set to "application/json" (in the Startup.cs)...
            // .. That means that the string "result" that's already in JsonFormat will be formatted again..
            // .. which leads to a wrong response containing escaped "-Character (\")
            return Ok(JsonConvert.DeserializeObject(result));
        }

        /// <summary>
        /// Creates a new submodel element
        /// </summary>
        /// <param name="body">Requested submodel element</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="content">Determines the request or response kind of the resource</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <response code="201">Submodel element created successfully</response>
        [HttpPost]
        [Route("{submodelIdentifier}/submodel/submodel-elements")]
        [ValidateModelState]
        [SwaggerOperation("PostSubmodelElementSubmodelRepo")]
        [Consumes("application/json")]
        [SwaggerResponse(statusCode: 201, type: typeof(ISubmodelElement),
            description: "Submodel element created successfully")]
        public virtual IActionResult PostSubmodelElementSubmodelRepo([FromBody] JObject body,
            [FromRoute] [Required] string submodelIdentifier, [FromQuery] string level, [FromQuery] string content,
            [FromQuery] string extent)
        {
            var bodyParsed = JsonNode.Parse(body.ToString());
            var submodelElement = Jsonization.Deserialize.ISubmodelElementFrom(bodyParsed);

            submodelIdentifier = System.Web.HttpUtility.UrlDecode(submodelIdentifier);

            _repository.CreateSubmodelElement(submodelIdentifier, submodelElement);
            
            return Ok();
        }
    }
}
