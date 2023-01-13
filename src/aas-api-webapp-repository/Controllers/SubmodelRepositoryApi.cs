using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using AAS.API.Repository;
using Aas.Api.Repository.Attributes;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using AasCore.Aas3_0_RC02;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

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
            var result = await _repository.GetAllSubmodels();
            return Ok(result);
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
        public async Task<ActionResult<Submodel>> GetSubmodelSubmodelRepo([FromRoute][Required] string submodelIdentifier, [FromQuery] string level, [FromQuery] string content, [FromQuery] string extent)
        {
            var Message = $"About page visited at {DateTime.UtcNow.ToLongTimeString()}";
            _logger.LogInformation(Message);
            submodelIdentifier = System.Web.HttpUtility.UrlDecode(submodelIdentifier);
            return Ok(await _repository.GetSubmodelWithId(submodelIdentifier));
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
        [Consumes("application/json")]
        [Route("{submodelIdentifier}/submodel/submodel-elements")]
        [ValidateModelState]
        [SwaggerOperation("PostSubmodelElementSubmodelRepo")]
        [SwaggerResponse(statusCode: 201, type: typeof(ISubmodelElement),
            description: "Submodel element created successfully")]
        public virtual IActionResult PostSubmodelElementSubmodelRepo([FromBody] dynamic body,
            [FromRoute] [Required] string submodelIdentifier, [FromQuery] string level, [FromQuery] string content,
            [FromQuery] string extent)
        {
            if (body.modelType == "Property")
            {
                Property property = new JObjectToPropertyMapper(_mapper).map(body);
                _repository.CreateSubmodelElement(submodelIdentifier,property);
            }
            return Ok();
        }
    }
}
