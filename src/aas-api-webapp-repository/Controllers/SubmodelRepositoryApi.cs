using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AAS.API.Registry.Attributes;
using AAS.API.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using Submodel = AAS.API.Models.Submodel;

namespace AAS.API.Registry.Controllers
{
    [Route("api/v1/submodels")]
    [ApiController]
    public class SubmodelRepositoryApi : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISubmodelRepository _repository;
        private readonly IMapper _mapper;


        public SubmodelRepositoryApi(IConfiguration config, ISubmodelRepository repository, IMapper mapper)
        {
            _configuration = config ??
                             throw new ArgumentNullException(nameof(config));

            _repository = repository ??
                          throw new ArgumentNullException(nameof(repository));
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
            submodelIdentifier = System.Web.HttpUtility.UrlDecode(submodelIdentifier);
            return Ok(await _repository.GetSubmodelWithId(submodelIdentifier));
        }
    }
}
